using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class GroupInfoViewModel
    {
        public IList<Lecture> Lectures { get; set; }
        public IList<IUser> Students { get; set; }
        public Group Group { get; set; }
    }
}
