using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class AddFileViewModel
    {
        public string ReturnUrl { get; set; }

       [Required]
        [UIHint("File")]
        public IFormFile File{ get; set; }


    }
    public class AddDocFileViewModel : AddFileViewModel
    {
        [Required]
        public int PageID { get; set; }

        public string Desription { get; set; }
    }
    public class AddTaskFileViewModel : AddFileViewModel
    {
        [Required]
        public int TaskID { get; set; }

        public string Desription { get; set; }
    }
}
