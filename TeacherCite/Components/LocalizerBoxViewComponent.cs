using Ganss.XSS;
using KovalukApp.Models;
using KovalukApp.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Components
{
    [ViewComponent]
    public class LocalizerBoxViewComponent : ViewComponent
    {
        
        public LocalizerBoxViewComponent()
        {
           
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            ViewBag.Culture = CultureInfo.CurrentCulture;
            return View();
        }
    }
}
