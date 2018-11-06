using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using KovalukApp.Models;
using KovalukApp.Models.ViewModels;
using Ganss.XSS;
using Microsoft.EntityFrameworkCore;

namespace KovalukApp.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles ="Teacher")]
    public class AccountController : Controller
    {
       
        private UserManager<User> Manager;
        private RoleManager<IdentityRole> Roles;
        private ApplicationContext context;
        private SignInManager<User> SignInManager;
        private AppMailSender MailSender;
        IDocStorage Docs;
        IGroupsRepository Groups;
        public AccountController( UserManager<User> manag, RoleManager<IdentityRole> roles
            ,ApplicationContext ctx, SignInManager<User> signInManager,
            AppMailSender Sender,
            IGroupsRepository groups,
            IDocStorage docs)
        {
            context = ctx;
            Manager = manag;
            Roles = roles;
            SignInManager = signInManager;
            MailSender = Sender;
            Docs = docs;
            Groups = groups;

        }
        public async Task<IActionResult> StudentProfile(string returnUrl,string username)
        {
            var sanitizer = new HtmlSanitizer();
            var user = await Manager.FindByNameAsync(username);
            if (user==null&&!(user is StudentUser))
            {
                return LocalRedirect(returnUrl);
            }
            var model = new StudentProgressViewModel();
            model.User = user;
            model.returnUrl = returnUrl;
            model.Tasks = Docs.GetStudentTasks(user.Id) ?? new List<StTask>();
            model.Group = Groups.GetGroupById(((StudentUser)user).GroupID);
            model.UnvisitedLectures = Groups.Lectures.Where(x => x.Date < DateTime.Now && !x.Visits.Any(y => y.VisitorID == user.Id) && x.GroupID == model.Group.GroupID).AsNoTracking().ToList() ?? new List<Lecture>();
            model.VisitedLectures = Groups.Lectures.Where(x => x.Date < DateTime.Now && x.Visits.Any(y => y.VisitorID == user.Id) && x.GroupID == model.Group.GroupID).AsNoTracking().ToList() ?? new List<Lecture>();
            model.HtmlData = sanitizer.Sanitize(CommonMark.CommonMarkConverter.Convert(user.Description ?? "### No description"));


            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string username,string returnUrl = "/")
        {
            var user = await Manager.FindByNameAsync(username);
            var roles = await Manager.GetRolesAsync(user);
            var studentData = new EditViewModel
            {
                Avatar = user.Avatar,
                ReturnUrl = returnUrl,
                UserName = user.UserName,
                OldUserName=username,
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
        public async Task<IActionResult> Avatar(IFormFile file,string username)
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
            return Json(false);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditViewModel edited)
        {

            var user = await Manager.FindByNameAsync(edited.OldUserName);

            if (ModelState.IsValid)
            {



                user.Email = edited.Email;
                user.Description = edited.Description;
                user.PhoneNumber = edited.PhoneNumber;
                user.UserName = edited.UserName;
                user.FirstName = edited.FirstName;
                user.LastName = edited.LastName;
              


                var result = await Manager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    if (User.Identity.Name==edited.OldUserName&&edited.OldUserName!=edited.UserName)
                    {
                       await SignInManager.RefreshSignInAsync(user);
    }
                    return LocalRedirect(edited.ReturnUrl);
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

        [HttpGet]
        public IActionResult CreateStudentAccount(int GroupID, string returnUrl)
        {
            var model = new CreateStudentAccountViewModel { GroupID = GroupID, returnUrl = returnUrl };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CreateStudentAccount(CreateStudentAccountViewModel model)
        {
            if (context.Students.Any(x=>x.NumberOfStudentBook==model.NumberOfStudentBook))
            {
                ModelState.AddModelError("",$"Student with number {model.NumberOfStudentBook} is alredy exist.");
            }

            if (ModelState.IsValid)
            {
                StudentUser student = new StudentUser() {
                    UserName = model.FirstName + "_" + model.SecondName+"_"+model.NumberOfStudentBook,
                    Email = model.Email,
                    GroupID = model.GroupID,
                    FirstName = model.FirstName,
                    LastName = model.SecondName,
                    NumberOfStudentBook = model.NumberOfStudentBook };

               
                var result= await Manager.CreateAsync(student);
                if (result.Succeeded)
                {
                    var user = await Manager.FindByEmailAsync(student.Email) as StudentUser;

                    try
                    {
                        var token = await Manager.GenerateEmailConfirmationTokenAsync(user);

                        string link = $@"{(HttpContext.Request.IsHttps?"https":"http")}://{HttpContext.Request.Host}{Url.Action("Registration", "Account", new { area = "Student", Token = token, Magic = student.Id })}";
                        var mailmodel = new StRgsMailModel { user = user, link = link };
                        var msg = MailSender.HtmlEmailMesage("EmailRegistration", mailmodel);
                
                        await MailSender.SendEmailAsync(student.Email, "Registration", msg);


                        await Manager.AddToRoleAsync(student, "Student");

                        return Redirect(model.returnUrl);
                    }
                    catch (Exception e)
                    {
                        await Manager.DeleteAsync(user);
                        ModelState.AddModelError("",e.Message);
                    }
                   
                    
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("",item.Description);
                    }
                }
            }
            return View(model);
        }

     
       [HttpPost]
       [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAccount(string username,string returnUrl)
        {
            var user =await Manager.FindByNameAsync(username);
            if (!(await Manager.IsInRoleAsync(user,"Teacher")))
            {
                foreach (var item in context.Tasks.Where(x => x.UserID == user.Id))
                {
                    context.RemoveRange(context.Answers.Where(x=>x.StTaskID==item.StTaskID));
                    context.RemoveRange(context.TaskFiles.Where(x=>x.StTaskId==item.StTaskID));
                }
                context.RemoveRange(context.Tasks.Where(x=>x.UserID==user.Id));
                context.RemoveRange(context.Visits.Where(x=>x.VisitorID==user.Id));
                context.SaveChanges();
                await Manager.DeleteAsync(user);
            }
            return LocalRedirect(returnUrl);
        }
    }
}