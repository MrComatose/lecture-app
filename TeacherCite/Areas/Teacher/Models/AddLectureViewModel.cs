using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class AddLectureViewModel
    {
        [Required]
        [Display(Name ="Date of the lecture:")]
        public DateTime Date { get; set; }

        public int GroupID { get; set; }

        [Required]
        [Display(Name ="Place of lecture:")]
        public string Place { get; set; }

        [Required]
        [Display(Name ="Description:")]
        public string Description { get; set; }

        
        public string returnUrl { get; set; }
    }
}
