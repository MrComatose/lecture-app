using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class DocFile : AppFile
    {
        public int DocPageId { get; set; }
     
        public string Description { get; set; }
    }
}
