using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class ShowPublicPageViewModel
    {
        public int DocPageID { get; set; }
        public IList<DocFile> Files { get; set; }
        public string HtmlDataText { get; set; }
        public string returnUrl { get; set; }
        public string Date { get; set; }
        public string Name { get; set; }
    }
    public class ShowPrivatePageViewModel :ShowPublicPageViewModel{
       
        public int GroupID { get; set; }
        public IList<TaskModel> Tasks { get; set; }
   
    }
    public class TaskModel
    {
        public StTask Task { get; set; }
        public IUser User { get; set; }
    }
}
