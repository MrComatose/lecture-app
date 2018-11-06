using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class AppFile
    {
        public string FileName { get; set; }

        public byte[] Value { get; set; }

        public string FileSize { get; set; }

        public string FileExtansion { get; set; }

        public int AppFileID { get; set; }


    }
}
