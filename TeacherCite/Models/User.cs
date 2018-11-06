
using Microsoft.AspNetCore.Identity;
using System;

namespace KovalukApp.Models
{
    

   
        public class User : IdentityUser ,IUser
         {
        
            public string Description { get; set; }

            public byte[] Avatar { get; set; }

            public bool BLocked { get; set; }

            public string FirstName { get; set; }
        
            public string LastName { get; set; }
        }
   
}
