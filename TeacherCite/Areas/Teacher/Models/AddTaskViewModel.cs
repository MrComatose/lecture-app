using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class AddTaskViewModel
    {
        [Required]
        public int DocPageID { get; set; }

        public string returnUrl { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int MaxCost { get; set; }

        [Required]
        public bool IsForAllStudents { get; set; }

        [Required]
        public DateTime DeadLine { get; set; }

    }
}
