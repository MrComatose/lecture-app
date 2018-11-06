using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class AddGroupViewModel
    {
        [Required]
        [StringLength(300)]
        public string Description { get; set; }

        [Required]
        [Display(Name ="Year")]
        [Range(0,6)]
        public int YearOfStudy { get; set; }
        [Display(Name ="Name")]
        [Required]
        public string GroupName { get; set; }
        
        public string returnUrl { get; set; }

    }

}
