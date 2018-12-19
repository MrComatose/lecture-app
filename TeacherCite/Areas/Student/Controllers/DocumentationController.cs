using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ganss.XSS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;

namespace KovalukApp.Areas.Student.Controllers
{
    
    [Area("Student")]
    [Authorize(Roles ="Student")]
    public class DocumentationController : Controller
    {
        UserManager<User> Manager;
        SignInManager<User> SignInManager;
        IDocStorage Docs;
        IFileStorage Files;
        IGroupsRepository Groups;
        IHostingEnvironment environment;
        public DocumentationController(UserManager<User> manager, SignInManager<User> signInManager, IDocStorage docs, IFileStorage files,IGroupsRepository groups, IHostingEnvironment env)
        {
            Manager = manager;SignInManager = signInManager;Docs = docs;Files = files;
            Groups = groups;
            environment = env;
        }
        public IActionResult List(string returnUrl)
        {
            var student = Manager.Users.FirstOrDefault(x=>x.UserName==User.Identity.Name) as StudentUser;
            var pages = Docs.Pages.Where(x=>x.GroupID==student.GroupID||x.GroupID==0);
            var model = new DocsViewModel {
                UserData=student,
                GroupName= Groups.Groups.FirstOrDefault(x => x.GroupID == student.GroupID)?.GroupName,
                Pages=pages.ToList(),
                returnUrl=returnUrl
            };
            return View(model);
        }
        public IActionResult ShowPage( int PageID,string returnUrl="/")
        {
         
            var student = Manager.Users.FirstOrDefault(x => x.UserName == User.Identity.Name) as StudentUser;
            var page = Docs.Pages.FirstOrDefault(x => (x.GroupID == student.GroupID || x.GroupID == 0) && x.DocPageID == PageID);
            if (page==null)
            {
                return LocalRedirect(returnUrl);
            }
            var freetasks = Docs.Tasks.Where(x=>x.UserID==null && x.DocPageID == PageID);
            var task = Docs.Tasks.FirstOrDefault(x=>x.UserID==student.Id&&x.DocPageID==PageID);


           
            var directory = Directory.GetFiles(environment.ContentRootPath + @"/wwwroot/FileExtansions");
            var files = Files.DocumentationFiles.Where(x => x.DocPageId == PageID); new List<AppFile>();
            var Model = new List<DocFileData>();
            foreach (var item in files)
            {
                var Size = String.Format(new FileSizeFormatProvider(), "{0:fs}", item.Value.LongLength);
                var FileName = item.FileName;
                var ext = new String(FileName.TakeLast(FileName.Length - (FileName.LastIndexOf('.') == -1 ? FileName.Length : FileName.LastIndexOf('.')) - 1).ToArray());
                if (!directory.Any(x => x.Contains(ext)))
                {
                    ext = "";
                }
                Model.Add(new DocFileData(FileName, ext, Size,item.Description));
            }


            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Add("class");
            var Html = sanitizer.Sanitize(CommonMark.CommonMarkConverter.Convert(page.Data));
            var model = new StDocViewModel {
                UserData=student, Files=Model
                ,FreeTasks=freetasks.ToList(),PersonalTasks=task,
                Page=page,returnUrl=returnUrl,Html=Html
            };
            
            return View(model);
        }

        static object TaskLocker = new object();
        public async Task<IActionResult> ChooseTask(int StTaskID, string returnUrl)
        {
            var user = await Manager.FindByNameAsync(User.Identity.Name);
            lock (TaskLocker)
            {
              var task=  Docs.Tasks.FirstOrDefault(x=>x.StTaskID==StTaskID&&x.UserID==null);
                
                if (task!=null)
                {
                    var isNoElseTasksForThisUser = Docs.Tasks
                        .FirstOrDefault(x => x.UserID == user.Id && x.DocPageID == task.DocPageID)==null;
                    var isDocPageFromThisGroup = Docs.Pages
                        .FirstOrDefault(x => x.DocPageID == task.DocPageID).GroupID == (user as StudentUser).GroupID;
                    if (isNoElseTasksForThisUser&&isDocPageFromThisGroup)
                    {
                        task.UserID = user.Id;
                        Docs.UpdateTask(task);
                    }
                    else
                    {
                        return View("TaskError", returnUrl);
                    }
                   
                }else
                {
                    return View("TaskError",returnUrl);
                }
            return LocalRedirect(returnUrl);
            }
        }
    }

}