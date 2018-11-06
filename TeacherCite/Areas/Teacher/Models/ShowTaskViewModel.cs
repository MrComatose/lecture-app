using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class ShowTaskViewModel
    {
        public string returnUrl { get; set; }
        public StTask Task { get; set; }
        public IUser User { get; set; }
        public List<AnswerViewModel> Answers {get;set;}
        public List<StTaskFileData> Files { get; set; }
    }
    public class AnswerViewModel
    {
        public Answer Answer { get; set; }
        public IUser User { get; set; }
        public bool IsTeacherAnswer { get; set; }
    }
    public class StTaskFileData : DocFileData {
         public StTaskFileData()
        {

        }
        public StTaskFileData(string fileName, string extansion, string size, string description)
        {
            FileName = fileName;
            Extansion = extansion;
            Size = size;
            Description = description;
        }
    }

}
