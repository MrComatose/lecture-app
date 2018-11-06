using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class PerformViewModel
    {
        public string returnUrl { get; set; }
        public Answer Answer{ get; set; }

    }
}
