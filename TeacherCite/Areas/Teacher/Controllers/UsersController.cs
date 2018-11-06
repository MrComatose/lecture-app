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
    [Authorize(Roles="Teacher")]
    public class UsersController : Controller
    {
        private IUserRepository Users;

        public UsersController(IUserRepository users)
        {
            Users = users;
        }

        public IActionResult List(int page=0)
        {
            UserListViewModel viewModel = new UserListViewModel {Users= Users.UsersData.Skip(page*5).Take(5).ToList(),
                PageNumber =page,Count = Users.UsersData.Count()
            };
            return View(viewModel);
        }
    }
}