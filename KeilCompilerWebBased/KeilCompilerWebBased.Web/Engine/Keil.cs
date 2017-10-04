using KeilCompilerWebBased.Web.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System;
using System.IO.Compression;

namespace KeilCompilerWebBased.Web.Engine
{
    public class Keil
    {
        public static KeilProjectFile KeilProjectFile { get; private set; }

        public List<IncludeDirectoryPath> UVProjectFileToIFileList(
            string uvprojPath)
        {
            XDocument xDoc = XDocument.Load(uvprojPath);

            StringBuilder result = new StringBuilder();
            List<Models.IncludeDirectoryPath> iFiles = new List<Models.IncludeDirectoryPath>();

            var includePathC51 = xDoc.Descendants("C51")
                .Descendants("IncludePath")
                .First()
                .Value
                .ToString();

            var includePathA51 = xDoc.Descendants("Ax51")
                .Descendants("IncludePath")
                .Last()
                .Value
                .ToString();

            var defaultBinPath = xDoc.Descendants("BinPath")
                .First()
                .Value
                .ToString();

            var listingPath = xDoc.Descendants("ListingPath")
                .First()
                .Value
                .ToString();

            var outputDirectory = xDoc.Descendants("OutputDirectory")
                .First()
                .Value
                .ToString();

            var outputName = xDoc.Descendants("OutputName")
                .First()
                .Value
                .ToString();

            xDoc.Descendants("File")
                .Select(f => new
                {
                    fileName = f.Element("FileName").Value,
                    fileType = f.Element("FileType").Value,
                    filePath = f.Element("FilePath").Value
                })
                .ToList()
                .ForEach(f =>
                {
                    if (f.fileType == "1")
                    {
                        result.AppendLine(
                            string.Format(
                                "\"{0}\" LARGE OBJECTADVANCED OPTIMIZE (9,SIZE) REGFILE " +
                                "({1}{2}.ORC) BROWSE ORDER NOINTVECTOR INCDIR({3}) " +
                                "DEBUG NOPRINT OBJECT({1}{4}.obj)",
                                f.filePath.Remove(0, 2),
                                outputDirectory,                                // default .orc path
                                outputName,                                     // default .orc name
                                includePathC51,                                 // include path
                                Path.GetFileNameWithoutExtension(f.fileName))); // .obj file name

                        iFiles.Add(
                            new Models.IncludeDirectoryPath(
                                string.Format(
                                    "{0}.__i",
                                    Path.GetFileNameWithoutExtension(f.fileName)),
                                    result.ToString()));

                        result.Clear();
                    }
                    if (f.fileType == "2")
                    {
                        result.AppendLine(
                            string.Format(
                                "\"{0}\" INCDIR({1}) SET (LARGE) DEBUG NOPRINT OBJECT({2}{3}.obj) EP",
                                f.filePath.Remove(0, 2),
                                includePathA51,                                 // include path for Ax51
                                outputDirectory,                                // .obj output directory
                                Path.GetFileNameWithoutExtension(f.fileName))); // .obj file name

                        iFiles.Add(
                            new Models.IncludeDirectoryPath(
                                string.Format("{0}._ia",
                                Path.GetFileNameWithoutExtension(f.fileName)),
                                result.ToString()));

                        result.Clear();
                    }
                });

            KeilProjectFile = new KeilProjectFile(
                includePathC51,
                includePathA51,
                defaultBinPath,
                listingPath,
                outputDirectory,
                outputName
            );

            return iFiles;
        }

        public bool GenerateIFile(
            IncludeDirectoryPath FileInclude,
            string TargetPath)
        {
            bool bRet = true;

            try
            {
                File.WriteAllText(
                    Path.Combine(TargetPath, FileInclude.FileName),
                    FileInclude.Content);
            }
            catch (System.Exception)
            {
                bRet = false;
            }

            return bRet;
        }

        public bool GenerateLNPFile(
            List<IncludeDirectoryPath> InclListdeDirectoryPath,
            string TargetPath)
        {
            bool bRet = true;

            StringBuilder sb = new StringBuilder();

            try
            {
                // foreach (IncludeDirectoryPath s in InclListdeDirectoryPath)
                // {
                //     sb.Append("COMMON {\"");
                //     sb.Append(KeilProjectFile.OutputDirectory);
                //     sb.Append(Path.GetFileNameWithoutExtension(s.FileName));
                //     sb.AppendLine(".obj\"},");
                // }

                for (int i = 0; i < InclListdeDirectoryPath.Count; ++i)
                {
                    sb.Append("COMMON {\"");
                    sb.Append(KeilProjectFile.OutputDirectory);
                    sb.Append(
                        Path.GetFileNameWithoutExtension(
                            InclListdeDirectoryPath[i].FileName));
                    sb.Append(".obj\"}");

                    if (i < InclListdeDirectoryPath.Count - 1)
                        sb.AppendLine(",");
                    else
                        sb.AppendLine("");
                }

                // Remove the last comma
                sb.Length = sb.Length - 1;

                sb.AppendLine(
                    string.Format(
                        "TO \"{0}{1}\"",
                        KeilProjectFile.OutputDirectory,
                        KeilProjectFile.OutputName
                ));

                File.WriteAllText(
                    Path.Combine(TargetPath, KeilProjectFile.OutputName + ".lnp"),
                    sb.ToString());
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                bRet = false;
            }

            return bRet;
        }

        public bool UnzipBaseCodeToDirectory(
            string ZipSource,
            string TargetDirectory)
        {
            bool boRet = true;
            // string ZipSource = @"D:\PrivateOS\MiniTG132.zip";

            try
            {
                ZipFile.ExtractToDirectory(ZipSource, TargetDirectory);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                boRet = false;
            }

            // ObjectFolder = Path.Combine(TargetDirectory, @"MiniTG132\TG132\Obj");
            return boRet;
        }

        public List<string> CompileAll(
            string folderpath,
            string targetpath,
            out string outputcommand)
        {
            StringBuilder sb = new StringBuilder();
            List<string> listStr = new List<string>();

            try
            {
                KeilProjectFile.BinPath = "/home/wisnu/.wine/drive_c/keil/c51/bin/";
                KeilProjectFile.OutputDirectory = "./TG132/Obj/";

                string strCmdText = "";

                // Get all *.__i and *._ia files
                string[] files = Directory.GetFiles(
                    folderpath,
                    @"*.*",
                    SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    if (Path.GetExtension(file) == ".__i" ||
                        Path.GetExtension(file) == "._ia")
                    {
                        string CompilerFile = Path.GetExtension(file) == ".__i" ?
                            "C51.exe" :
                            "A51.exe";

                        strCmdText = string.Format("-c \"cd {0} && wine {1}{2} @{3}{4}\"",
                            targetpath,
                            KeilProjectFile.BinPath,
                            CompilerFile,
                            KeilProjectFile.OutputDirectory,
                            Path.GetFileName(file));

                        // strCmdText = string.Format("-c \"wine {0}{1} @{2}/{3}\"",
                        //     KeilProjectFile.BinPath,
                        //     CompilerFile,
                        //     folderpath,
                        //     Path.GetFileName(file));

                        // strCmdText = "-c \"wine --version\"";

                        string errr;
                        string s = RunConsole(strCmdText, out errr);

                        if(errr != "")
                        {
                            sb.AppendLine(errr);
                            listStr.Add(errr);
                        }
                        if(s != "")
                        {
                            sb.AppendLine(s);
                            listStr.Add(s);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string ss = ex.Message;
            }
            finally
            {
                outputcommand = sb.ToString();

            }
            
            return listStr;
            
        }

        public List<string> BuildAll(
            string targetpath)
        {
            List<string> listStr = new List<string>();

            try
            {
                // Execute LX51.exe
                string strCmdText = String.Format(
                    "/C cd {0} & D: & \"{1}LX51.EXE\" @{2}{3}.LNP",
                    targetpath,
                    KeilProjectFile.BinPath,
                    KeilProjectFile.OutputDirectory,
                    KeilProjectFile.OutputName);

                string errr;
                string s = RunConsole(strCmdText, out errr);

                if(errr != "")
                {
                    listStr.Add(errr);
                }
                if(s != "")
                {
                    listStr.Add(s);
                }

                // Find the newest file name
                var directory = new DirectoryInfo(
                    Path.Combine(
                        targetpath,
                        KeilProjectFile.OutputDirectory));
                var myFile = (from f in directory.GetFiles()
                    orderby f.LastWriteTime descending
                    select f).First();

                string targetfilename = Path.GetFileNameWithoutExtension(myFile.Name);

                // Execute OHX51.exe
                strCmdText = String.Format(
                    "/C cd {0} & D: & \"{1}OHX51.EXE\" \"{2}{3}\" H386",
                    targetpath,
                    KeilProjectFile.BinPath,
                    KeilProjectFile.OutputDirectory,
                    targetfilename);

                s = RunConsole(strCmdText, out errr);

                if(errr != "")
                {
                    listStr.Add(errr);
                }
                if(s != "")
                {
                    listStr.Add(s);
                }
            }
            catch (Exception ex)
            {
                string ss = ex.Message;
            }

            return listStr;
        }

        public string RunConsole(
            string cmd,
            out string err
        )
        {
            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = cmd;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            //* Read the output (or the error)
            string output = process.StandardOutput.ReadToEnd();
            // Console.WriteLine(output);
            err = process.StandardError.ReadToEnd();
            // Console.WriteLine(err);
            process.WaitForExit();

            return output;
        }
    }
}