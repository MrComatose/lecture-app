using Ganss.XSS;
using KovalukApp.Models;
using KovalukApp.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Components
{
    [ViewComponent]
    public class TeacherProfileViewComponent : ViewComponent
    {
        UserManager<User> Manager;
        public TeacherProfileViewComponent(UserManager<User> manager)
        {
            Manager = manager;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user =(await Manager.GetUsersInRoleAsync("Teacher")).FirstOrDefault();
            var roles = await Manager.GetRolesAsync(user);
            var sanitizer = new HtmlSanitizer();
            var model = new ProfileViewModel()
            {
                UserData = user,
                Roles = roles,
                HtmlData = sanitizer.Sanitize(CommonMark.CommonMarkConverter.Convert(user.Description ?? "### No description"))
            };
            return View(model);
        }
    }
}
