using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;
using KovalukApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Ganss.XSS;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KovalukApp.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class AccountController : Controller
    {
        SignInManager<User> SignInManager;
        UserManager<User> Manager;
        RoleManager<IdentityRole> Roles;
        IDocStorage Docs;
        IGroupsRepository Groups;
        IFileStorage Files;
        IUserRepository Users;
        public AccountController(SignInManager<User> sim,
            UserManager<User> manag,
            RoleManager<IdentityRole> roles,
            IUserRepository users,
            IGroupsRepository groups,
            IDocStorage docs,
        IFileStorage files
            )
        {
            SignInManager = sim;
            Manager = manag;
            Roles = roles;
            Users = users;
            Docs = docs;
            Files = files;
            Groups = groups;
        }

        public async Task<IActionResult> StudentProfile(string returnUrl)
        {
            var sanitizer = new HtmlSanitizer();
            var user = await Manager.FindByNameAsync(this.User.Identity.Name);
            var model = new StudentProgressViewModel();
            model.User = user;
            model.returnUrl = returnUrl;
            model.Tasks = Docs.GetStudentTasks(user.Id) ??new List<StTask>();
            model.Group = Groups.GetGroupById(((StudentUser)user).GroupID);
            model.UnvisitedLectures = Groups.Lectures.Where(x => x.Date<DateTime.Now&&!x.Visits.Any(y => y.VisitorID == user.Id) && x.GroupID == model.Group.GroupID).AsNoTracking().ToList()??new List<Lecture>();
            model.VisitedLectures= Groups.Lectures.Where(x => x.Date < DateTime.Now && x.Visits.Any(y => y.VisitorID == user.Id) && x.GroupID == model.Group.GroupID).AsNoTracking().ToList() ?? new List<Lecture>();
            model.HtmlData = sanitizer.Sanitize(CommonMark.CommonMarkConverter.Convert(user.Description ?? "### No description"));


            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Registration(string Token, string Magic)
        {
            var user = await Manager.FindByIdAsync(Magic) as StudentUser;
            var checkEmail = await Manager.IsEmailConfirmedAsync(user);
            var checkPassword = await Manager.HasPasswordAsync(user);

            if (!(checkPassword))
            {
                if (!checkEmail)
                {
                    var result = await Manager.ConfirmEmailAsync(user, Token);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);

                        }
                    }
                }

                RegistrationViewModel model = new RegistrationViewModel()
                {
                    LastName = user.LastName,
                    FirstName = user.FirstName,
                    Email = user.Email,
                    NumberOfStudentBook = user.NumberOfStudentBook,
                    ID = Magic,
                    ConfirmEmailHash = Token
                };
                return View(model);
            }
            else
            {
                return LocalRedirect("/Account/Login");
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Registration(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {


                StudentUser user = await Manager.FindByIdAsync(model.ID) as StudentUser;

                user.UserName = model.UserName;
                user.PhoneNumber = model.PhoneNumber;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                var result = await Manager.UpdateAsync(user);
                var result1 = await Manager.ResetPasswordAsync(user, await Manager.GeneratePasswordResetTokenAsync(user), model.Password);


                if (result.Succeeded && result1.Succeeded)
                {

                }
                else
                {
                    if (!result.Succeeded)
                    {


                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }
                    foreach (var error in result1.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

            }
            return Redirect("/Account/Login");

        }
        [HttpGet]
        public async Task<IActionResult> Edit(string returnUrl = "/")
        {
            var user = await Manager.FindByNameAsync(User.Identity.Name);
            var roles = await Manager.GetRolesAsync(user);
            var studentData = new EditViewModel
            {
                Avatar = user.Avatar,
                ReturnUrl = returnUrl,
                UserName = user.UserName,
                OldUserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Description = user.Description,
                PhoneNumber = user.PhoneNumber,
                Roles = roles
            };
            return View(studentData);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditViewModel edited)
        {

            var user = await Manager.FindByNameAsync(User.Identity.Name);

            if (ModelState.IsValid)
            {



                user.Email = edited.Email;
                user.Description = edited.Description;
                user.PhoneNumber = edited.PhoneNumber;
                user.UserName = edited.UserName;
                user.FirstName = edited.FirstName;
                user.LastName = edited.LastName;
                if (edited.NewAvatar != null)
                {
                    using (var binaryReader = new BinaryReader(edited.NewAvatar.OpenReadStream()))
                    {
                        user.Avatar = binaryReader.ReadBytes((int)edited.NewAvatar.Length);
                    }
                }


                var result = await Manager.UpdateAsync(user);

                if (result.Succeeded)
                {

                    await SignInManager.RefreshSignInAsync(user);
                    return Redirect(edited.ReturnUrl);
                }
                else
                {

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

            }
            var roles = await Manager.GetRolesAsync(user);
            edited.Roles = roles;
            edited.Avatar = user.Avatar;
            return View(edited);
        }
        [HttpPost]
        public async Task<IActionResult> Avatar(IFormFile file, string username)
        {
            if (User.Identity.Name == username)
            {
                var user = await Manager.FindByNameAsync(username);
                using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                {
                    user.Avatar = binaryReader.ReadBytes((int)file.Length);
                }
                var result = await Manager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Json(true);
                }
            }
            return StatusCode(404);
        }
    }
}
