using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;

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
        public TaskController(IDocStorage docs, IFileStorage files, IGroupsRepository groups, ApplicationContext ctx)
        {
            Documentation = docs;
            Files = files;
            Repository = groups;
            Context = ctx;
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
                            Name = Model.Name,
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
            return View(Documentation.Tasks);
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
                var answer = new Answer() {
                    AnswerDate = DateTime.Now,
                    StTaskID = task.StTaskID,
                    TextData = model.Description,
                    UserID=Context.Users.FirstOrDefault(x=>x.UserName==User.Identity.Name).Id

                };
                Documentation.AddAnswer(answer);
                return LocalRedirect(model.returnUrl);
            }
            return View(model);
        }
    }
}