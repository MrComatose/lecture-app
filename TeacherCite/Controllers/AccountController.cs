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
        private AppMailSender EmailSender;
        public AccountController(SignInManager<User> sim, UserManager<User> manag,AppMailSender mail)
        {
            EmailSender = mail;
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
        public async Task<IActionResult> ResetPassword(string Email)
        {
            var user = await Manager.FindByEmailAsync(Email);
            if (user==null)
            {
                return StatusCode(404);
            }
            var token = await Manager.GeneratePasswordResetTokenAsync(user);
            string link = $@"{(HttpContext.Request.IsHttps ? "https" : "http")}://{HttpContext.Request.Host}{Url.Action("ChangePassword", "Account", new {  Token = token, Magic = user.Id })}";
            var mailmodel = new PasswordResetMailModel { user = user, link = link };
            var msg = EmailSender.HtmlEmailMesage("ResetPassword",mailmodel);
            await EmailSender.SendEmailAsync(user.Email,"Reset password",msg);
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> ChangePassword(string Token,string Magic)
        {
            var user = await Manager.FindByIdAsync(Magic);
            if (user==null)
            {
                return StatusCode(404);
            }
            var model = new ChangePasswordViewModel() {
                Token = Token, UserID = Magic
            };
            return View(model);
        }
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await Manager.FindByIdAsync(model.UserID);
            if (user == null)
            {
                return StatusCode(404);
            }
            var result = await Manager.ResetPasswordAsync(user,model.Token,model.Password);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("",item.Description);
                }
                return View(model);
            }

            return LocalRedirect("/Account/Login");
        }
    }
}
       
