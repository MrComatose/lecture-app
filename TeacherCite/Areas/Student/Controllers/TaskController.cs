using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace KovalukApp.Areas.Student.Controllers
{
    [Authorize(Roles ="Student")]
    [Area("Student")]
    public class TaskController : Controller
    {
      
        UserManager<User> Manager;
        SignInManager<User> SignInManager;
        IDocStorage Docs;
        IFileStorage Files;
        IGroupsRepository Groups;
        IHostingEnvironment environment;
        public TaskController(UserManager<User> manager, SignInManager<User> signInManager, IDocStorage docs, IFileStorage files, IGroupsRepository groups, IHostingEnvironment env)
        {
            Manager = manager; SignInManager = signInManager; Docs = docs; Files = files;
            Groups = groups;
            environment = env;
        }
        public async Task<IActionResult> List()
        {
            var user = await Manager.FindByNameAsync(User.Identity.Name);

            return View(Docs.Tasks.Where(x=>x.UserID==user.Id));
        }
        public async Task<IActionResult> ShowTask(string returnUrl, int StTaskID)
        {
            var task = Docs.Tasks.FirstOrDefault(x => x.StTaskID == StTaskID);
           
            if (task != null)
            {
                var StUser = await Manager.FindByNameAsync(User.Identity.Name);
                if (task.UserID!=StUser.Id)
                {
                    return LocalRedirect(returnUrl);
                }

                var answers = Docs.Answers.Where(x => x.StTaskID == StTaskID);
                var AnswerViewModels = new List<AnswerViewModel>();
                var FileViewModels = new List<StTaskFileData>();
                foreach (var item in answers)
                {
                    var user = await Manager.FindByIdAsync(item.UserID);
                    AnswerViewModels.Add(new AnswerViewModel
                    {
                        Answer = item,
                        User = user,
                        IsTeacherAnswer = StUser.Id == user.Id ? false : true
                        });
                }
                foreach (var item in Files.StudentTaskFiles.Where(x => x.StTaskId == task.StTaskID).ToList())
                {
                    FileViewModels.Add(new StTaskFileData(item.FileName, item.FileExtansion, item.FileSize, item.Description));
                }
                var model = new ShowTaskViewModel()
                {
                    returnUrl = returnUrl,
                    Answers = AnswerViewModels.OrderBy(x => x.Answer.AnswerDate).Reverse().ToList(),
                    User =StUser,
                    Task = task,
                    Files = FileViewModels
                };
                return View(model);
            }
            return LocalRedirect(returnUrl);
        }
        [HttpGet]
        public async Task<IActionResult> Perform(int TaskID, string returnUrl = "/")
        {
            var student = await Manager.FindByNameAsync(User.Identity.Name);
            var task = Docs.Tasks.FirstOrDefault(x=>x.StTaskID==TaskID&&x.UserID==student.Id);
            if (task == null)
            {
                return LocalRedirect(returnUrl);
            }
            var model = new PerformViewModel();
            model.returnUrl = returnUrl;
            model.Answer = new Answer { AnswerDate=DateTime.Now,StTaskID=TaskID,UserID=student.Id};
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Perform(PerformViewModel Model)
        {
            var student = await Manager.FindByNameAsync(User.Identity.Name);
            var task = Docs.Tasks.FirstOrDefault(x => x.StTaskID == Model.Answer.StTaskID && x.UserID == Model.Answer.UserID&& Model.Answer.UserID == student.Id);
            if (task==null)
            {

                return LocalRedirect(Model.returnUrl);
            }
            task.IsChecked = false;
            Docs.UpdateTask(task);
            Docs.AddAnswer(Model.Answer);
            return LocalRedirect(Model.returnUrl);
        }
        [HttpGet]
        public async Task<IActionResult> AddTaskFile(string returnUrl, int TaskID)
        {
            var student = await Manager.FindByNameAsync(User.Identity.Name);
            var task = Docs.Tasks.FirstOrDefault(x => x.StTaskID == TaskID && x.UserID == student.Id);
            if (task == null)
            {
                return LocalRedirect(returnUrl);
            }
            var model = new AddTaskFileViewModel { ReturnUrl = returnUrl, TaskID = TaskID };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddTaskFile(AddTaskFileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var student = await Manager.FindByNameAsync(User.Identity.Name);
                var task = Docs.Tasks.FirstOrDefault(x => x.StTaskID == model.TaskID && x.UserID == student.Id);
                if (task == null)
                {
                    return LocalRedirect(model.ReturnUrl);
                }
                if (Files.StudentTaskFiles.Any(x => x.FileName == model.File.FileName && x.StTaskId == model.TaskID))
                {
                    ModelState.AddModelError("", "File with name " + model.File.FileName + " is alredy exist.");
                    return View(model);
                }

                var directory = Directory.GetFiles(environment.ContentRootPath + @"/wwwroot/FileExtansions");

                var ext = new String(model.File.FileName.TakeLast(model.File.FileName.Length - (model.File.FileName.LastIndexOf('.') == -1 ? model.File.FileName.Length : model.File.FileName.LastIndexOf('.')) - 1).ToArray());
                if (!directory.Any(x => x == ("File" + ext + ".png")))
                {
                    ext = "";
                }
                var File = new TaskFile()
                {
                    FileName = model.File.FileName,
                    FileExtansion = ext,
                    StTaskId = model.TaskID,
                    Description = model.Desription
                };
                using (var reader = new BinaryReader(model.File.OpenReadStream()))
                {
                    byte[] data = reader.ReadBytes((int)model.File.Length);
                    File.FileSize = String.Format(new FileSizeFormatProvider(), "{0:fs}", data.LongLength);
                    File.Value = data;
                }

                Files.AddFile(File);
                task.IsChecked = false;
                Docs.AddAnswer(
                    new Answer {
                        AnswerDate = DateTime.Now,
                        StTaskID = model.TaskID,
                        UserID = student.Id,
                        TextData = $"Adding file {File.FileName} to task."
                    });
                return LocalRedirect(model.ReturnUrl);
            }
            return View(model);
        }
    }
}