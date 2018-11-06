using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class News
    {
        public int NewsID { get; set; }

        public string Name { get; set; }

        public string TextData { get; set; }

        public DateTime PublicationDate { get; set; }
    }
}
