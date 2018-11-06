using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class TaskFile:AppFile
    {
        public int StTaskId { get; set; }

        public string Description { get; set; }
    }
}
