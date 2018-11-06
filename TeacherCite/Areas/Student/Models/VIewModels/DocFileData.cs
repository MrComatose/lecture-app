using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class DocFileData:FileNameData
    {
        public string Description { get; set; }
        public DocFileData()
        {

        }
        public DocFileData(string fileName, string extansion, string size,string description)
        {
            FileName = fileName;
            Extansion = extansion;
            Size = size;
            Description = description;
        }
    }
}
