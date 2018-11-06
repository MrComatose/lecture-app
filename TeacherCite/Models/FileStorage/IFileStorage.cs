using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public interface IFileStorage
    {
        IQueryable<AppFile> AllFiles { get;  }

        IQueryable<TaskFile> StudentTaskFiles { get;  }

        IQueryable<DocFile> DocumentationFiles { get; }

        void AddTaskFile(TaskFile file);
        void AddDocFile(DocFile file);
        void AddFile(AppFile file);

        void RemoveFile(AppFile file);

        void UpdateFile(AppFile file);

    }
}
