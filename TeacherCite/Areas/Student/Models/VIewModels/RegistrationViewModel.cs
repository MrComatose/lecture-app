using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class RegistrationViewModel
    {
        public string ConfirmEmailHash { get; set; }
        public string ID { get; set; }
        public string Email { get; set; }
        public int NumberOfStudentBook { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }


        [Required()]
        [Phone(ErrorMessage ="Phone number is incirrect.")]
        [StringLength(13,MinimumLength =10)]
        [UIHint("Phone")]
        public string PhoneNumber { get; set; } = "+380";

        [Required]
        [UIHint("Password")]
        public string Password { get; set; }

        [Required]
        [UIHint("Password Confirm")]
        [Compare("Password", ErrorMessage = "Password != Password Confirm")]
        public string PasswordConfirm { get; set; }

     
        [Required]
        [UIHint("User Name")]
        [MaxLength(30,ErrorMessage ="Max lenght of username is 30 symbols")]
        public string UserName { get; set; }

  
        public string ReturnUrl { get; set; }
    }
}
