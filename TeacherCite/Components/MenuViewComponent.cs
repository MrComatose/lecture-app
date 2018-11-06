using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KovalukApp.Models;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KovalukApp.ViewComponents
{
    [ViewComponent]
    public class MenuViewComponent : ViewComponent
    {
        IUserRepository Manager;
        public MenuViewComponent(IUserRepository manager)
        {
            Manager = manager;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = Manager.FindUserByName(User.Identity.Name);
                if (user==null)
                {

                }
                if (User.IsInRole("Student"))
                {
                    return View("MenuStudent", user);
                }
                if (User.IsInRole("Admin"))
                {
                    return View("MenuAdmin", user);
                }
                if (User.IsInRole("Teacher"))
                {
                    return View("MenuTeacher", user);
                }

            }
         
            
            return View();
        }
    }
}
