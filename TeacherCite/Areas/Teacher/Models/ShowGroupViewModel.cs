using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace KovalukApp.Models
{
    public class ShowGroupViewModel
    {
        public List<IUser> Users { get; set; }

        public Group Group{get;set;}

        public List<Lecture> Lectures { get; set; }

        public string returnUrl { get; set; }

    }
}
