using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public interface IUser
    {
        byte[] Avatar { get; set; }
        string Email { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string UserName { get; set; }
        string Description { get; set; }
        string PhoneNumber { get; set; }
        string Id { get; set; }
    
    }
}
