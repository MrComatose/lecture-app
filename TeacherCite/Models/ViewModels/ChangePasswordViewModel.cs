using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class ChangePasswordViewModel
    {
        [Required]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Password != Password Confirm")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string UserID { get; set; }
        [Required]
        public string Token { get; set; }
    }
}
