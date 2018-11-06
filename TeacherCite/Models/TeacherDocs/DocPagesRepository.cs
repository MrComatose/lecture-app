using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class DocPagesRepository:IDocStorage
    {
        ApplicationContext context;
        public DocPagesRepository(ApplicationContext ctx)
        {
            context = ctx;
        }

        public IQueryable<DocPage> Pages => context.Documentation;

        public IQueryable<StTask> Tasks => context.Tasks;

        public IQueryable<Answer> Answers => context.Answers;

        public void AddAnswer(Answer answer)
        {
            context.Add(answer);
            context.SaveChanges();
        }

        public void AddPage(DocPage page)
        {
            context.Add(page);
            context.SaveChanges();
        }

        public void AddTask(StTask task)
        {
            context.Add(task);
            context.SaveChanges();
        }

        public IList<DocPage> GetGroupDocs(int GroupID)
        {
            return context.Documentation.Where(x=>x.GroupID==GroupID).ToList();
        }

        public IList<StTask> GetStudentTasks(string UserID)
        {
            return context.Tasks.Where(x=>x.UserID==UserID).ToList();
        }

        public void RemoveAnswer(Answer answer)
        {
            context.Remove(answer);
            context.SaveChanges();
        }

        public void RemovePage(DocPage page)
        {
            
            context.Remove(page);
          context.SaveChanges();
        }

        public void RemoveTask(StTask task)
        {
            
            context.RemoveRange(context.Answers.Where(x => x.StTaskID == task.StTaskID));
            context.RemoveRange(context.TaskFiles.Where(x => x.StTaskId == task.StTaskID));
            context.Remove(task);
            context.SaveChanges();
        }

        public void UpdatePage(DocPage page)
        {

            if (page.DocPageID == 0)
            {
                context.Add(page);
            }
            context.SaveChanges(); ;
        }

        public void UpdateTask(StTask task)
        {

            if (task.StTaskID == 0)
            {
                context.Add(task);
            }
            context.SaveChanges();
        }
    }
}
