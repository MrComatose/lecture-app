using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ganss.XSS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;
using KovalukApp.Models.ViewModels;

namespace KovalukApp.Controllers
{
    
    public class AccountController : Controller
    {
        private SignInManager<User> SignInManager;
        private UserManager<User> Manager;
        public AccountController(SignInManager<User> sim, UserManager<User> manag)
        {
            SignInManager = sim;
            Manager = manag;
        }
        public async Task<IActionResult> Logout()
        {
            
            await SignInManager.SignOutAsync();
            return Redirect("/");
        }

        [HttpGet]
   
        public IActionResult Login(string returnUrl = "/")
        {


            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        [Authorize]
        public async Task<IActionResult> Profile(string returnUrl,string username)
        {
            var user = await Manager.FindByNameAsync(username);
            var roles =await Manager.GetRolesAsync(user);
            var sanitizer = new HtmlSanitizer();
            var model = new ProfileViewModel()
            {
                UserData=user,
                returnUrl=returnUrl,
                Roles=roles,
                HtmlData= sanitizer.Sanitize(CommonMark.CommonMarkConverter.Convert(user.Description??"### No description"))
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel details)
        {
            if (ModelState.IsValid)
            {
                var User = await Manager.FindByEmailAsync(details.Email);
                if (User != null)
                {
                    await SignInManager.SignOutAsync();
                    var signinresult = await SignInManager.PasswordSignInAsync(User, details.Password, details.RememberMe, false);
                    if (signinresult.Succeeded)
                    {
                        return LocalRedirect(details.ReturnUrl ?? "/");
                    }
                }
                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(details);
        }
    }
}
       
