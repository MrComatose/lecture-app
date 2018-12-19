using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KovalukApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KovalukApp.Areas.Teacher.Components
{
    [ViewComponent]
    [Authorize(Roles ="Teacher")]
    public class UserListViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(List<IUser> users)
        {
            return View(users);
        }
    }
}