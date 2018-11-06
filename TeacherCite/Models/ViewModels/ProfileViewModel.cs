using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models.ViewModels
{
    public class ProfileViewModel
    {
        public User UserData { get; set; }
        public string returnUrl { get; set; }
        public IList<string> Roles { get; set; }
        public string HtmlData { get; set; }
    }
}
