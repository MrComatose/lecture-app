using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public interface IVisitPost
    {
        string Username { get; set; }
         int LectureID { get; set; }
         bool Was { get; set; }
    }
    public class ShowLectureViewModel:IVisitPost
    {
        public string Username { get; set; }
        public int LectureID { get; set; }
        public bool Was { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Number { get; set; }
    }
    public class ShowLectureList
    {
        public IEnumerable<ShowLectureViewModel> list { get; set; }
        public string returnUrl { get; set; }
    }
}
