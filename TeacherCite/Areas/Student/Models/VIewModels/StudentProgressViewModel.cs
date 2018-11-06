using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class StudentProgressViewModel
    {
        public string returnUrl { get; set; }
        public User User { get; set; }
        public IList<Lecture> UnvisitedLectures { get; set; }
        public IList<Lecture> VisitedLectures { get; set; }
        public IList<StTask> Tasks { get; set; }
        public string HtmlData { get; set; }
        public Group Group { get; set; }
    }
}
