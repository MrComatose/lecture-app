using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class UserListViewModel
    {
       public  List<IUser> Users { get; set; }

        public int Count { get; set; }

        public int PageNumber { get; set; }
    }
}
