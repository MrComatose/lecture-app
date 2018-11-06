using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class EFFileStorage : IFileStorage
    {
        ApplicationContext context;
        public EFFileStorage(ApplicationContext db)
        {
            context = db;
        }

        public IQueryable<AppFile> AllFiles => context.Files;

        public IQueryable<TaskFile> StudentTaskFiles => context.TaskFiles;

        public IQueryable<DocFile> DocumentationFiles => context.DocFiles;

        public void AddDocFile(DocFile file)
        {

            context.Add(file);
            context.SaveChanges();
        }

        public void AddFile(AppFile file)
        {
            context.Add(file);
            context.SaveChanges();
        }

        public void AddTaskFile(TaskFile file)
        {
            context.Add(file);
            context.SaveChanges();
        }

        public void RemoveFile(AppFile file)
        {
            context.Remove(file);
            context.SaveChanges();
        }

        public void UpdateFile(AppFile file)
        {
            if (file.AppFileID==0)
            {
                context.Add(file);
            }
            context.SaveChanges();
        }
    }
}
