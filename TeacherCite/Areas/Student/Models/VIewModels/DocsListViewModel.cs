using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class StDocViewModel
    {
        public IUser UserData { get; set; }
        public IList<StTask> FreeTasks { get; set; }
        public StTask PersonalTasks { get; set; }
        public IList<DocFileData> Files { get; set; }
        public DocPage Page { get; set; }
        public string Html { get;set; }
        public string returnUrl { get; set; }
    }
    public class DocsViewModel
    {
        public IUser UserData { get; set; }
        public string GroupName { get; set; }
        public IList<DocPage> Pages { get; set; }
        public string returnUrl { get; set; }
    }

}
