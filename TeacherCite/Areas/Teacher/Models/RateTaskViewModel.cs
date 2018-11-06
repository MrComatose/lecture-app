using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class RateTaskViewModel
    {
        public string returnUrl { get; set; }
        public int TaskID { get; set; }
        public int CurrentRate { get; set; }
        public int MaxRate { get; set; }
        public string Description { get; set; }
    }
}
