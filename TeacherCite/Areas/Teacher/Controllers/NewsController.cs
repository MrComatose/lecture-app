using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KovalukApp.Models;
namespace KovalukApp.Areas.Teacher.Controllers
{
  [Authorize(Roles ="Teacher")]
  [Area("Teacher")]
    public class NewsController : Controller
    {
        INewsStorage News;
        IFileStorage Files;
        IHostingEnvironment environment;
        public NewsController(INewsStorage news,IFileStorage files, IHostingEnvironment env)
        {
            News = news;
            Files = files;
            environment = env;
        }

        [HttpPost]
        public IActionResult AddNews(string name,string txtdata,IFormFile file)
        {
           
            if (file.ContentType.Contains("image")&&name!=null&&txtdata!= null)
            {
                string ext = "png";
                var File = new AppFile()
                {
                    FileName = (name + '.' + ext),
                    FileExtansion = ext
                };
               
              
                using (var reader = new BinaryReader(file.OpenReadStream()))
                {
                    byte[] data = reader.ReadBytes((int)file.Length);
                    File.FileSize = String.Format(new FileSizeFormatProvider(), "{0:fs}", data.LongLength);
                    File.Value = data;
                }

                Files.AddFile(File);
                News.AddNews(new News() { Name = name, TextData = txtdata, PublicationDate = DateTime.Now });
                return Ok();
            }
            return StatusCode(404);
        }

        public IActionResult List(string returnUrl)
        {
            ViewBag.Return = returnUrl;
            return View(News.AllNews.AsNoTracking().OrderByDescending(x=>x.PublicationDate).ToList() ?? new List<News>());
        }
        public IActionResult RemoveNews(string returnUrl,int NewsID)
        {
            var news = News.AllNews.First(x => x.NewsID == NewsID);
            var file = Files.AllFiles.FirstOrDefault(x => x.FileName == news.Name + ".png");
            if (file!=null)
            {
                Files.RemoveFile(file);
            }
            
            News.DeleteNews(news);
       
            return LocalRedirect(returnUrl);
        }

    }
}