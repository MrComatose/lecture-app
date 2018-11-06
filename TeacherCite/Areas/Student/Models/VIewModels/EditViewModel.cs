using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KovalukApp.Models;

namespace KovalukApp.Models
{
    
    public class EditViewModel
    {
        
      public string OldUserName { get; set; }

        public IEnumerable<string> Roles { get; set; }


        [Required]
        [EmailAddress]
        [Display(Name = "Електронна адреса:")]
        [UIHint("Email")]
        public string Email { get; set; }


        [Required()]
        [Phone(ErrorMessage = "Phone number is incirrect.")]
        [StringLength(13, MinimumLength = 10)]
        [UIHint("Phone")]
        public string PhoneNumber { get; set; } = "+380";

        [UIHint("Description")]
        [StringLength(500, MinimumLength = 0, ErrorMessage = "Description is to long")]
        public string Description { get; set; }

        [UIHint("Avatar")]
        public IFormFile NewAvatar { get; set; }

      
        [UIHint("Avatar")]
        public byte[] Avatar { get; set; }

        [Required]
        [UIHint("User Name")]
        public string UserName { get; set; }
        
        [Required]
        [UIHint("First Name")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "First Name is incorrect")]
        public string FirstName { get; set; }

        [Required]
        [UIHint("Second Name")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Second Name is incorrect")]
        public string LastName { get; set; }

        public string ReturnUrl { get; set; }
    }
}
