using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;

namespace KovalukApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Teacher, Admin")]
    public class StudentsController : Controller
    {
        IGroupsRepository repository;
        UserManager<User> manager;
        public StudentsController(IGroupsRepository repo,UserManager<User> mng)
        {
            repository = repo;
            manager = mng;
        }
        
        [HttpPost]
        public async Task<IActionResult> ChekVisit(string UserName,int LectureID)
        {
            var user = await manager.FindByNameAsync(UserName);
            var visit = repository.Visits.FirstOrDefault(
                x => x.LectureID == LectureID && x.VisitorID == user.Id
                );
            var lection = repository.Lectures.FirstOrDefault(x => x.LectureID == LectureID);
            if (visit==null)
            {
                repository.AddVisitor(lection, user.Id);
            }
            else {
                repository.DeletVisit(visit);
            }
            return Ok();
        }
    }
}