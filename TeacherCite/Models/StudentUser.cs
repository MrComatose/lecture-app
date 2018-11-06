using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class StudentUser:User
    {
       public int NumberOfStudentBook { get; set; }
        public int GroupID { get; set; }
    }
}
