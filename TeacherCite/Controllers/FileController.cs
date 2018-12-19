using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;

namespace KovalukApp.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        IFileStorage repository;
        IUserRepository Users;
        INewsStorage News;
        public FileController(IFileStorage repo,IUserRepository users,INewsStorage news)
        {
            repository = repo;
            Users = users;
            News = news;
        }
        public IActionResult Photo(string id)
        {
            return File(Users.UsersData.FirstOrDefault(x=>x.UserName==id).Avatar, "image/jpeg");
        }
        public IActionResult GetFile(string filename)
        {
            return File(repository.AllFiles.FirstOrDefault(x => x.FileName == filename).Value, "application/octet-stream", filename);
        }
        [AllowAnonymous]
        public IActionResult NewsPhoto(int NewsID)
        {
            var filename = News.AllNews.First(x=>x.NewsID==NewsID).Name+".png";
            var bytes = repository.AllFiles.FirstOrDefault(x => x.FileName == filename)?.Value;
            if (bytes==null)
            {
                return StatusCode(404);
            }else
            return File(bytes, "application/octet-stream", filename);
        }
    }
}