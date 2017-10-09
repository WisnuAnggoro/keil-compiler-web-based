using System.Collections.Generic;
using KeilCompilerWebBased.Web.Models;

namespace KeilCompilerWebBased.Web.ViewModel
{
    public class SourceCodeFilesViewModel
    {
        public List<SourceCodeFile> SourceCodeFileList { get; set; }

        public SourceCodeFilesViewModel()
        {
            
        }

        public SourceCodeFilesViewModel(
            List<SourceCodeFile> list
        )
        {
            SourceCodeFileList = new List<SourceCodeFile>(list);
        }
    }
}