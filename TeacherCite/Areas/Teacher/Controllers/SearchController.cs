using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KovalukApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KovalukApp.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles="Teacher")]
    public class SearchController : Controller
    {
        IUserRepository UserData;
        IDocStorage Documentation;
        public SearchController(IUserRepository users,IDocStorage docs)
        {
            UserData = users;
            Documentation = docs;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Users(string str)
        {
            IList<IUser> users = new List<IUser>();
            if (!(str==null||str==String.Empty))
            {
                foreach (var item in UserData.UsersData
                   .Where(x=>
                   x.UserName.Contains(str)||x.FirstName.Contains(str)||x.LastName.Contains(str)
                   ||x.Email.Contains(str)||x.PhoneNumber.Contains(str)||
                   (x.FirstName+" "+x.LastName).Contains(str)

                   ))
                {
                    users.Add(item);
                }
               

                ViewBag.String = str;
                return View(users);
            }
            else { 
            return View(UserData.UsersData.Take(5).ToList());
            }
        }
        public IActionResult Tasks(string str)
        {
            IList<StTask> tasks = new List<StTask>();
            
            if (!(str == null || str == String.Empty))
            {
                var strings = str.Split(" ");

                foreach (var item in strings)
                {
                    var taskcollection = Documentation.Tasks.Where(x=>x.Name.Contains(item));
                    foreach (var task in taskcollection)
                    {
                        if (task != null && !tasks.Contains(task))
                        {
                            tasks.Add(task);
                        }
                    }
                  
                }


                ViewBag.String = str;
                
            }
            return View(tasks);
        }
    }
}
