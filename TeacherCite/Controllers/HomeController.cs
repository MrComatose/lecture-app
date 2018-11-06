using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;
namespace KovalukApp.Controllers
{
    public class HomeController : Controller
    {
        INewsStorage News;
        public HomeController(INewsStorage news)
        {
            News = news;
        }
        public IActionResult Main()
        {
            return View(News.GetSixLatestNews()??new List<News>());
           
        }
    }
}