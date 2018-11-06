using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class DocumentationsListViewModel
    {
       public List<Group> Groups { get; set; }
        public IList<DocPage> Pages { get; set; }
    }
}
