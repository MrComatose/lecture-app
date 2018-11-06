using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;

namespace KovalukApp.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles ="Teacher")]
    public class GroupController : Controller
    {
        private IGroupsRepository repository;
        private ApplicationContext context;
        public GroupController(IGroupsRepository groups, ApplicationContext ctx)
        {
            context= ctx;
            repository = groups;
        }
        [HttpGet]
        public IActionResult Add(string returnUrl)
        {
            var model = new AddGroupViewModel() {returnUrl=returnUrl };
            return View(model);
        }
        [HttpPost]
        public IActionResult Add(AddGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool check = repository.Groups.Any(x=>x.GroupName==model.GroupName);
                if (check)
                {
                    ModelState.AddModelError("",$"Group with name {model.GroupName} is alredy exist.");
                }
                else
                {
                    Group group= new Group {GroupName=model.GroupName,Description=model.Description,YearOfStudy=model.YearOfStudy };

                    repository.SaveGroup(group);
                    return RedirectToAction(nameof(List));
                }

            }
            return View(model);
        }
        public IActionResult List()
        {
            return View(repository.Groups.ToList()??new List<Group>());
        }
      
        public IActionResult ShowGroup(int GroupID=1,string returnUrl="/")
        {
            var model = new ShowGroupViewModel { Users = context.Students.Where(x => x.GroupID == GroupID).ToList<IUser>()??new List<IUser>(),
                Lectures = repository.Lectures.Where(x => x.GroupID == GroupID).ToList()??new List<Lecture>(), returnUrl = returnUrl
                , Group = repository.GetGroupById(GroupID)
            };
            return View(model);
        }
        [HttpGet]
        public IActionResult AddLecture(string returnUrl,int GroupID)
        {
            ViewBag.GroupName = repository.Groups.FirstOrDefault(x => x.GroupID == GroupID).GroupName;
            var model = new AddLectureViewModel { GroupID = GroupID, returnUrl = returnUrl };
            return View(model);
        }
        [HttpPost]
        public IActionResult AddLecture(AddLectureViewModel model)
        {
            var lecture = new Lecture() { Place=model.Place,Date=model.Date, Description=model.Description};
            repository.AddLecture(repository.GetGroupById(model.GroupID),lecture);
           return LocalRedirect(model.returnUrl);
        }
        public IActionResult RemoveLecture(int LectureID,string returnUrl)
        {
            repository.DeleteLecture(repository.GetLectureById(LectureID));
            return LocalRedirect(returnUrl);
        }
        public IActionResult ShowLecture(int LectureID,string returnUrl="/")
        {
            var groupid=repository.Lectures.First(x=>x.LectureID==LectureID).GroupID;
            List<StudentUser> users = context.Students.Where(x=>x.GroupID==groupid).ToList();
            List<ShowLectureViewModel> model = new List<ShowLectureViewModel>();
            foreach (var user in users)
            {
                ShowLectureViewModel showmodel = new ShowLectureViewModel
                {
                    Email = user.Email,
                    FirstName=user.FirstName,
                    LastName=user.LastName,
                    Username=user.UserName,
                    LectureID=LectureID,
                    Number=user.NumberOfStudentBook,
                    Was=repository.Visits.Any(x=>x.VisitorID==user.Id&&x.LectureID==LectureID)
                
                };
                model.Add(showmodel);
            }
            model.OrderBy(x=>x.Number);

            var ShowModel = new ShowLectureList() {list=model,returnUrl=returnUrl };


            return View(ShowModel);
        }

        public IActionResult RemoveGroup(int GroupId)
        {
            var group = repository.GetGroupById(GroupId);
           
            repository.DeleteGroup(group);
            return RedirectToAction("List");
        }
      
    }
}