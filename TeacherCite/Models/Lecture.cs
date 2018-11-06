using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class Lecture
    {
        public int LectureID { get; set; }

        public DateTime Date { get; set; }

        public int GroupID { get; set; }

        public string Place { get; set; }

        public ICollection<Visit> Visits { get; set; }

        public string Description { get; set; }
    }
}
