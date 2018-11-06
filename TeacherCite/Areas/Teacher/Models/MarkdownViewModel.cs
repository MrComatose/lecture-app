using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class MarkdownViewModel
    {
        public string returnUrl { get; set; }


        public string MarkdownData { get; set; }

        public int PageID { get; set; }
        [Required]
        public string PageName { get; set; }
    }
}
