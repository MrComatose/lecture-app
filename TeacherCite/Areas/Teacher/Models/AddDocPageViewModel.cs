using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class AddDocPageViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string GroupName { get; set; }

        public int GroupID { get; set; }

        public string returnUrl { get; set; }
    }
}
