using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommonMark;
using Ganss.XSS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;

namespace KovalukApp.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles="Teacher")]
    public class DocumentationController : Controller
    {
        IDocStorage Documentation;
        IFileStorage Files;
        IGroupsRepository Repository;
        IUserRepository Users;
        IHostingEnvironment environment;
        public DocumentationController(IDocStorage docs,IFileStorage files,IGroupsRepository groups,IUserRepository users, IHostingEnvironment env)
        {
            Documentation = docs;
            Files = files;
            Repository = groups;
            Users = users;
            environment = env;
        }
        public IActionResult RemovePage(string returnUrl, int PageID)
        {
            Documentation.RemovePage(Documentation.Pages.FirstOrDefault(x=>x.DocPageID==PageID));
            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        public IActionResult AddMarkdownToPage(string returnUrl,int PageID)
        {
            var page = Documentation
                .Pages.FirstOrDefault(x => x.DocPageID == PageID);
            var model = new MarkdownViewModel {
                returnUrl =returnUrl,
                MarkdownData = page.Data,
                PageID=PageID,
                PageName=page.Name
            };
            return View(model);
        }
        [HttpPost]
        public IActionResult AddMarkdownToPage(MarkdownViewModel model)
        {
            //if (model.MarkdownData?.Contains("<script>")==true)
            //{
            //    ModelState.AddModelError("","Unsafe tag <script>...</script>");
            //}
            if (ModelState.IsValid)
            {
              var page=Documentation.Pages.FirstOrDefault(x => x.DocPageID == model.PageID);
                page.Data = model.MarkdownData;
                page.Name = model.PageName;
                Documentation.UpdatePage(page);
                return LocalRedirect(model.returnUrl);
            }
            return View(model);
        }
        public IActionResult ShowPage(int PageID,string returnUrl)
        {
           
            var docpage = Documentation.Pages.FirstOrDefault(x => x.DocPageID == PageID);
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Add( "class");
            if (docpage.GroupID==0)
            {
               
           
               
                var model = new ShowPublicPageViewModel
                {
                    Name=docpage.Name,
                    returnUrl=returnUrl,
                    Files =Files.DocumentationFiles.Where(x=>x.DocPageId==PageID).ToList(),
                    DocPageID = PageID,
                    HtmlDataText= sanitizer.Sanitize(CommonMark.CommonMarkConverter.Convert(docpage.Data?? @"### No content ")),
                    Date=docpage.Date.ToShortDateString()
                };
                return View("ShowPublicDocPage", model);
            }
            else {
                var List = new List<TaskModel>();
                foreach (var item in Documentation.Tasks.Where(x => x.DocPageID == PageID).ToList())
                {
                    List.Add(new TaskModel() {Task=item,User=Users.UsersData.FirstOrDefault(x=>x.Id==item.UserID) });
                }
                var model = new ShowPrivatePageViewModel {
                    Name = docpage.Name,
                    returnUrl = returnUrl,
                    Files = Files.DocumentationFiles.Where(x => x.DocPageId == PageID).ToList(),
                    DocPageID = PageID,
                    HtmlDataText = sanitizer.Sanitize(CommonMark.CommonMarkConverter.Convert(docpage.Data ?? @"### No content ")),
                    Date = docpage.Date.ToShortDateString(),
                    GroupID = docpage.GroupID,
                    Tasks =List
            };
            return View("ShowPrivateDocPage",model);
            }
        }
        [HttpGet]
        public IActionResult AddDocPage(string returnUrl,int GroupID)
        {
            var model =new AddDocPageViewModel{returnUrl=returnUrl,GroupID=GroupID,GroupName=Repository.Groups.FirstOrDefault(x=>x.GroupID==GroupID)?.GroupName??"Public" };
            return View(model);
        }
        [HttpPost]
        public IActionResult AddDocPage(AddDocPageViewModel model)
        {
            if (ModelState.IsValid)
            {
                Documentation.AddPage(new DocPage {GroupID=model.GroupID,Name=model.Name,Date=model.Date });
                return LocalRedirect(model.returnUrl);
            }
            return View(model);
        }
        public IActionResult List()
        {
            var model = new DocumentationsListViewModel {

                Pages = (Documentation.Pages.ToList() ?? new List<DocPage>()).OrderBy(x => x.Date).Reverse().ToList(),
                Groups = Repository.Groups.ToList()??new List<Group>()
            };
           
            return View(model);
        }

        [HttpGet]
        public IActionResult AddDocFile(string returnUrl,int PageID)
        {
            var model = new AddDocFileViewModel {ReturnUrl=returnUrl,PageID=PageID };
            return View(model);
        }
        [HttpPost]
        public IActionResult AddDocFile(AddDocFileViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (Files.AllFiles.Any(x => x.FileName == model.File.FileName))
                {
                    ModelState.AddModelError("", "File with name " + model.File.FileName + " is alredy exist.");
                    return View(model);
                }

                var directory = Directory.GetFiles(environment.ContentRootPath + @"/wwwroot/FileExtansions");

                var ext = new String(model.File.FileName.TakeLast(model.File.FileName.Length - (model.File.FileName.LastIndexOf('.') == -1 ? model.File.FileName.Length : model.File.FileName.LastIndexOf('.')) - 1).ToArray());
                if (!directory.Any(x => x==("File"+ext+".png")))
                {
                    ext = "";
                }
                var File = new DocFile()
                {
                    FileName = model.File.FileName,
                    FileExtansion = ext,
                    DocPageId=model.PageID,
                    Description=model.Desription
                };
                using (var reader = new BinaryReader(model.File.OpenReadStream()))
                {
                    byte[] data = reader.ReadBytes((int)model.File.Length);
                    File.FileSize = String.Format(new FileSizeFormatProvider(), "{0:fs}", data.LongLength);
                    File.Value = data;
                }

                Files.AddFile(File);
                return LocalRedirect(model.ReturnUrl);
            }
            return View(model);
        } 
    }
}