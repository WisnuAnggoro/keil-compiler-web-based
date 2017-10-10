using KeilCompilerWebBased.Web.Models;
using System.Collections.Generic;

namespace KeilCompilerWebBased.Web.ViewModel
{
    public class OSTypeViewModel
    {
        public string SelectedOSType { get; set; }
        public List<OSType> OSTypeList { get; set; }
    }
}