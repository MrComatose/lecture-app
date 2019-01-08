using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace KovalukApp.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "Teacher")]
    public class TaskController : Controller
    {
        IDocStorage Documentation;
        IFileStorage Files;
        IGroupsRepository Repository;
        ApplicationContext Context;
        IHostingEnvironment environment;
        public TaskController(IDocStorage docs, 
            IFileStorage files,
            IGroupsRepository groups, 
            ApplicationContext ctx,
            IHostingEnvironment env
            )
        {
            Documentation = docs;
            Files = files;
            Repository = groups;
            Context = ctx;
            environment = env;
        }

        [HttpGet]
        public IActionResult AddTask(string returnUrl, int PageID)
        {
            var model = new AddTaskViewModel() { returnUrl = returnUrl, DocPageID = PageID };
            return View(model);
        }
        [HttpPost]
        public IActionResult AddTask(AddTaskViewModel Model)
        {

            if (ModelState.IsValid)
            {
                if (Model.IsForAllStudents)
                {
                    var groupID = Documentation.Pages.FirstOrDefault(x => x.DocPageID == Model.DocPageID).GroupID;
                    var studentsFromThisGroup = Context.Students.Where(x => x.GroupID == groupID).ToList();
                    foreach (var student in studentsFromThisGroup)
                    {
                        var Task = new StTask
                        {
                            CurrentCost = 0,
                            DeadLine = Model.DeadLine,
                            Description = Model.Description,
                            IsChecked = true,
                            UserID = student.Id,
                            Name = Model.Name+" "+student.NumberOfStudentBook,
                            MaxCost = Model.MaxCost,
                            DocPageID = Model.DocPageID
                        };
                        Documentation.AddTask(Task);
                    }
                }
                else
                {
                    var Task = new StTask
                    {
                        CurrentCost = 0,
                        DeadLine = Model.DeadLine,
                        Description = Model.Description,
                        IsChecked = true,
                        Name = Model.Name,
                        MaxCost = Model.MaxCost,
                        DocPageID = Model.DocPageID
                    };
                    Documentation.AddTask(Task);
                }
                return LocalRedirect(Model.returnUrl);
            }
            return View(Model);
        }
        public IActionResult List()
        {
            var model = new List<TaskModel>();
            foreach (var task in Documentation.Tasks)
            {
                var taskmodel = new TaskModel()
                {
                    Task = task,
                    User = Context.Students.FirstOrDefault(x=>x.Id==task.UserID)
                };
                model.Add(taskmodel);
            }
            return View(model);
        }
        public IActionResult UnChecked()
        {
            return Json(Documentation.Tasks.Where(x=>!x.IsChecked).Count());
        }
        public IActionResult RemoveTask(string returnUrl, int StTaskID)
        {
            Documentation.RemoveTask(Documentation.Tasks.FirstOrDefault(x => x.StTaskID == StTaskID));
            return LocalRedirect(returnUrl);
        }

        public IActionResult ShowTask(string returnUrl, int StTaskID)
        {
            var task = Context.Tasks.FirstOrDefault(x => x.StTaskID == StTaskID);
            if (task != null)
            {


                var answers = Documentation.Answers.Where(x => x.StTaskID == StTaskID);
                var AnswerViewModels = new List<AnswerViewModel>();
                var FileViewModels = new List<StTaskFileData>();
                foreach (var item in answers)
                {
                    var user = Context.Users.FirstOrDefault(x => x.Id == item.UserID);
                    AnswerViewModels.Add(new AnswerViewModel
                    {
                        Answer = item,
                        User = user,
                        IsTeacherAnswer = Context.UserRoles.Any(x => x.UserId == user.Id & Context.Roles.FirstOrDefault(y => x.RoleId == y.Id).Name == "Teacher")
                    });
                }
                foreach (var item in Files.StudentTaskFiles.Where(x => x.StTaskId == task.StTaskID).ToList())
                {
                    FileViewModels.Add(new StTaskFileData(item.FileName,item.FileExtansion,item.FileSize,item.Description));
                }
                var model = new ShowTaskViewModel()
                {
                    returnUrl = returnUrl,
                    Answers = AnswerViewModels.OrderBy(x=>x.Answer.AnswerDate).Reverse().ToList(),
                    User = Context.Users.FirstOrDefault(x => x.Id == task.UserID),
                    Task = task,
                    Files = FileViewModels
                };
                return View(model);
            }
            return LocalRedirect(returnUrl);
        }
        [HttpGet]
        public IActionResult Rate(string returnUrl,int TaskID)
        {
            var task = Documentation.Tasks.FirstOrDefault(x=>x.StTaskID==TaskID);
            if (task!=null)
            {

            var model = new RateTaskViewModel {
                returnUrl=returnUrl,
                TaskID=TaskID,
                CurrentRate=task.CurrentCost,
                MaxRate=task.MaxCost,
                
            };
            return View(model);

            }
            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public IActionResult Rate(RateTaskViewModel model)
        {
            var task = Documentation.Tasks.FirstOrDefault(x => x.StTaskID == model.TaskID);

            if (task.MaxCost<model.CurrentRate)
            {
                ModelState.AddModelError("","Rate cannot be more than "+ model.MaxRate);
            }
            if (ModelState.IsValid)
            {
                 var oldrate = task.CurrentCost;
             
                task.CurrentCost = model.CurrentRate;
                task.IsChecked = true;
                Documentation.UpdateTask(task);
                if (model.CurrentRate != oldrate)
                {
                   

                    var answer = new Answer()
                    {
                        AnswerDate = DateTime.Now,
                        StTaskID = task.StTaskID,
                        TextData = $@"Оцінка була змінена з {oldrate} до {model.CurrentRate}",
                        UserID = Context.Users.FirstOrDefault(x => x.UserName == User.Identity.Name).Id

                    };

                    Documentation.AddAnswer(answer);
                }
                if (!String.IsNullOrEmpty(model.Description))
                {
                    
                    var answer = new Answer()
                    {
                        AnswerDate = DateTime.Now,
                        StTaskID = task.StTaskID,
                        TextData = model.Description,
                        UserID = Context.Users.FirstOrDefault(x => x.UserName == User.Identity.Name).Id

                    };

                    Documentation.AddAnswer(answer);
                }
                return LocalRedirect(model.returnUrl);
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> AddTaskFile(string returnUrl, int TaskID)
        {
            
            var task = Documentation.Tasks.FirstOrDefault(x => x.StTaskID == TaskID );
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
                var teacherID = Context.Users.FirstOrDefault(x=>x.UserName==this.User.Identity.Name).Id;
                 var task = Documentation.Tasks.FirstOrDefault(x => x.StTaskID == model.TaskID);
                if (task == null)
                {
                    return LocalRedirect(model.ReturnUrl);
                }
                if (Files.StudentTaskFiles.Any(x => x.FileName == model.File.FileName&&x.StTaskId==model.TaskID))
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
                task.IsChecked = true;
                Documentation.AddAnswer(
                    new Answer
                    {
                        AnswerDate = DateTime.Now,
                        StTaskID = model.TaskID,
                        UserID = teacherID,
                        TextData = $"Adding file {File.FileName} to task."
                    });
                return LocalRedirect(model.ReturnUrl);
            }
            return View(model);
        }
    
        public IActionResult DeleteAnswer(int AnswerID,string returnUrl)
        {
            Documentation
                .RemoveAnswer(Documentation.Answers.FirstOrDefault(x=>x.AnswerID==AnswerID));
            return LocalRedirect(returnUrl);
        }


}
}