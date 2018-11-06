using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class StTask
    {
        public int StTaskID { get; set; }

        public int DocPageID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string UserID { get; set; }

        public int MaxCost { get; set; }

        public bool IsChecked { get; set; }

        public DateTime DeadLine { get; set; }

        public int CurrentCost { get; set; }
    }
}
