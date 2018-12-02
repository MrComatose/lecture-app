using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace KovalukApp.Areas.Student.Controllers
{
    [Authorize(Roles ="Student")]
    [Area("Student")]
    public class GroupController : Controller
    {
        private IGroupsRepository Groups;
        private UserManager<User> Manager;
        private ApplicationContext Context;
        public GroupController(IGroupsRepository groups, UserManager<User> manager,ApplicationContext ctx)
        {
            Groups=groups;
            Manager = manager;
            Context=ctx;
        }
        public async Task<IActionResult> List()
        {
            var user = await Manager.FindByNameAsync(this.User.Identity.Name) as StudentUser;
            var group = Groups.GetGroupById(user.GroupID);
            var lectures = Groups.Lectures.Where(x=>x.GroupID==group.GroupID).OrderByDescending(x=>x.Date).ToList();
            var model = new GroupInfoViewModel {
                Lectures = lectures,
                Group = group,
                Students = Context.Students.Where(x=>x.GroupID==group.GroupID).ToList<IUser>()
            };
            return View(model);
        }
    }
}