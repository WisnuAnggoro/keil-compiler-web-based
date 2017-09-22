namespace KeilCompilerWebBased.Web.Models
{
    public class IncludeDirectoryPath
    {
        public string FileName { get; set; }
        public string Content { get; set; }

        public IncludeDirectoryPath(string FileName, string Content)
        {
            this.FileName = FileName;
            this.Content = Content;
        }         
    }
}