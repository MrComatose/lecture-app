using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
   public interface IDocStorage
    {
        IQueryable<DocPage> Pages { get; }
        IQueryable<StTask> Tasks { get; }
        IQueryable<Answer> Answers { get; }
        void AddTask(StTask task);
        void RemoveTask(StTask task);
        void AddPage(DocPage page);
        void RemovePage(DocPage page);
        void AddAnswer(Answer answer);
        void RemoveAnswer(Answer answer);
        void UpdatePage(DocPage page);
        void UpdateTask(StTask task);
        

        IList<StTask> GetStudentTasks(string UserID);

        IList<DocPage> GetGroupDocs(int GroupID);
    }
}
