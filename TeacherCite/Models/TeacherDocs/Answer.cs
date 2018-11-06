using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class Answer
    {
        public int StTaskID { get; set; }
        public string UserID { get; set; }
        public string TextData { get; set; }
        public int AnswerID { get; set; }
        public DateTime AnswerDate { get; set; }
    }
}
