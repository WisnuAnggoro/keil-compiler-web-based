namespace KeilCompilerWebBased.Web.Models
{
    public class KeilProjectFile
    {
        public string IncludePathC51 { get; set; }
        public string IncludePathA51 { get; set; }
        public string BinPath { get; set; }
        public string ListingPath { get; set; }
        public string OutputDirectory { get; set; }
        public string OutputName { get; set; }

        public KeilProjectFile(
            string IncludePathC51,
            string IncludePathA51,
            string BinPath,
            string ListingPath,
            string OutputDirectory,
            string OutputName)
        {
            this.IncludePathC51 = IncludePathC51;
            this.IncludePathA51 = IncludePathA51;
            this.BinPath = BinPath;
            this.ListingPath =ListingPath;
            this.OutputDirectory = OutputDirectory;
            this.OutputName = OutputName;
        }
    }
}