using KeilCompilerWebBased.Web.Engine;
using KeilCompilerWebBased.Web.Models;
using KeilCompilerWebBased.Web.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using Microsoft.Extensions.FileProviders;

namespace KeilCompilerWebBased.Web.Controllers
{
    public class ExecuteController : Controller
    {
        private IConfiguration _configuration;

        // Root path, like '/' in linux
        private static string _rootPath;

        // Path to get Keil executable file
        private static string _keilBinPath;

        // Path dedicated per session
        private static string _sessionPath;
        
        // Title of selected project,
        // used in Compiling and Building process
        private static string _titleProject;

        // Path to get source project
        private static string _sourcePath;

        // Path to create the HEX file
        private static string _objectPath;

        // Relative path of output directory
        private static string _outDirRelative;

        // Containing list of *.__i files
        private static List<IncludeDirectoryPath> listFile;

        // List of available OS source
        private static List<OSType> _osTypeList;
        // Selected OS Index retrieved from user input
        private static int _selectedOsIndex;

        // Accessing all method in Keil class
        private Keil _keil;

        public ExecuteController(IConfiguration configuration)
        {
            _configuration = configuration;
            _rootPath = _configuration["RootPath"];
            _keilBinPath = _configuration["KeilBinPath"];
            _keil = new Keil();

            // Initialize the available OS source
            _osTypeList = new List<OSType>();
            _osTypeList.Add(new OSType { Id = 1, Name = "Mini OS on TG132" });
            _osTypeList.Add(new OSType { Id = 2, Name = "USIM OS on TG132" });
            _osTypeList.Add(new OSType { Id = 3, Name = "USIM OS on W9F4" });
        }

        public IActionResult Index()
        {
            OSTypeViewModel osTypeViewModel = new OSTypeViewModel();
            osTypeViewModel.OSTypeList = _osTypeList;
            osTypeViewModel.SelectedOSType = String.Empty;

            // ViewBag.connectionstring = _configuration["RootPath"];
            // if (TempData["ErrorOsSelectionMessage"] != null)
            //     ViewBag.ErrorOsSelection = TempData["ErrorOsSelectionMessage"].ToString();
            return View(osTypeViewModel);
        }

        [HttpPost]
        public IActionResult Index(OSTypeViewModel OsTypeViewModel)
        {
            if (OsTypeViewModel.SelectedOSType != null)
            {
                // I'm strongly confident to use Parse instead of TryParse
                _selectedOsIndex = Int32.Parse(OsTypeViewModel.SelectedOSType);

                // Int32.TryParse(
                //     OsTypeViewModel.SelectedOSType,
                //     out iSelectOs);

                // return RedirectToAction(
                //     "UploadSources",
                //     new { SelectedOsIndex = _selectedOsIndex });

                return RedirectToAction(
                    "UploadSources");
            }
            else
            {
                // TempData["ErrorOsSelectionMessage"] = "Please select one of the above options!";
                return RedirectToAction("Index", "Execute");
            }
        }

        // public IActionResult SelectOSChip(OSTypeViewModel OsTypeViewModel)
        // {
        //     return View(OsTypeViewModel);
        // }

        public IActionResult UploadSources()
        {
            // Previous action that successfully performed
            // ViewBag.SelectedOsType = _osTypeList[SelectedOsIndex - 1].Name;
            ViewBag.SelectedOsType = _osTypeList[_selectedOsIndex - 1].Name;

            // Init project based on SelectedOsIndex
            bool isSuccess = InitProject(_selectedOsIndex);

            if (!isSuccess)
            {
                // If error occurs, user can't do anything
                // Better call the Admin
                ViewBag.ErrorMessage =
                    "Unable to initialize project. " +
                    TempData["ErrorInitProjectMessage"];

                return View("Error");
            }

            if (TempData["ErrorUploadSourceMessage"] != null)
                ViewBag.ErrorUploadSource = TempData["ErrorUploadSourceMessage"].ToString();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadSources(
            List<IFormFile> SourceCodeFiles)
        {
            List<SourceCodeFile> uploadedSourceCodeFileList =
                    new List<SourceCodeFile>();

            long size = SourceCodeFiles.Sum(f => f.Length);

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            foreach (var formFile in SourceCodeFiles)
            {
                if (formFile.Length > 0)
                {
                    uploadedSourceCodeFileList.Add(
                        new SourceCodeFile()
                        {
                            FileName = formFile.FileName,
                            FilePath = filePath
                        }
                    );

                    using (var stream = new FileStream(
                        filePath,
                        FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            if (size > 0)
            {
                SourceCodeFilesViewModel sourceCodeVM =
                    new SourceCodeFilesViewModel(
                        uploadedSourceCodeFileList
                    );

                return View(
                    "CompileAndBuild",
                    sourceCodeVM);

                // TempData["UploadedSourceCodeList"] = sourceCodeVM;

                // return RedirectToAction(
                //     "CompileAndBuild"
                //     // , new SourceCodeFilesViewModel(){
                //     //     SourceCodeFileList = uploadedSourceCodeFileList}
                //         );

                // return View("CompileAndBuild");
            }
            else
            {
                TempData["ErrorUploadSourceMessage"] =
                    "Please select at least one file before press Upload button!";

                // return View(
                //     "UploadSources",
                //     new { SelectedOsIndex = _selectedOsIndex });

                return RedirectToAction(
                    "UploadSources");
            }
        }

        public IActionResult CompileAndBuild(
            SourceCodeFilesViewModel sourceCodeFilesVM
        )
        {
            ViewBag.SelectedOsType = _osTypeList[_selectedOsIndex - 1].Name;

            // SourceCodeFilesViewModel sourceCodeFilesVM =
            //     (SourceCodeFilesViewModel)TempData["UploadedSourceCodeList"];
            return View(sourceCodeFilesVM);
        }

        [HttpPost]
        public IActionResult CompileAndBuild()
        {
            bool res = RunProcess();
            return View(
                "DownloadFiles");
        }

        public IActionResult DownloadFiles(
            bool boResult)
        {
            if(boResult)
            {
                ViewBag.DownloadFilesReady = "Success";
            }
            else
            {
                ViewBag.DownloadFilesReady = "Fail";
            }
            return View();
        }

        // [HttpPost]
        // public IActionResult DownloadFiles(
        //     )
        // {
            
        //     return View();
        // }

        public IActionResult DownloadHex()
        {
            try
            {
                string filePath = _objectPath;

                // Find Hex File
                string[] hexFiles = Directory
                    .GetFiles(_objectPath, "*.hex")
                    .Select(Path.GetFileName)
                    .ToArray();

                string fileName = hexFiles[0];

                IFileProvider provider = new PhysicalFileProvider(
                    filePath);
                IFileInfo fileInfo = provider.GetFileInfo(fileName);
                var readStream = fileInfo.CreateReadStream();
                var mimeType = "text/plain";
                return File(readStream, mimeType, fileName);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage =
                    "Unable to create hex files. " +
                    ex.Message;

                return View("Error");
            }
        }

        private bool InitProject(int ProjectNumber)
        {
            // Project Number is 1 based
            // BaseCodeList is 0 based
            string BaseCodeList = "BaseCodeList:" + (ProjectNumber - 1);

            try
            {
                // string argument = "BaseCodeList:0:BasePath";

                _titleProject = _configuration[BaseCodeList + ":Title"];

                _sessionPath = Path.Combine(
                    _rootPath,
                    Path.GetFileNameWithoutExtension(
                        Path.GetRandomFileName()));

                if (!Directory.Exists(_sessionPath))
                    Directory.CreateDirectory(
                        _sessionPath);

                _sourcePath = _configuration[BaseCodeList + ":BasePath"];
                _objectPath = Path.Combine(
                    _sessionPath,
                    _configuration[BaseCodeList + ":ObjPath"]);

                // Preparing source code
                _keil.UnzipBaseCodeToDirectory(
                    _configuration[BaseCodeList + ":ZipPath"],
                    _sessionPath);

                // Setting output relative directory for compiler and builder
                _outDirRelative = _configuration[BaseCodeList + ":OutDirRelative"];

                listFile = new List<IncludeDirectoryPath>();
                string sUvProjFilePath = String.Format(
                    "{0}/{1}/{2}",
                    _sessionPath,
                    _configuration[BaseCodeList + ":Title"],
                    _configuration[BaseCodeList + ":FileName"]
                );
                // $@"{_sessionPath}/{_configuration["BaseCodeList:0:Title"]}/{_configuration["BaseCodeList:0:FileName"]}";
                listFile = _keil.UVProjectFileToIFileList(
                    sUvProjFilePath);

                foreach (var file in listFile)
                {
                    _keil.GenerateIFile(
                        file,
                        _objectPath);
                }


                return true;
            }
            catch (Exception ex)
            {
                TempData["ErrorInitProjectMessage"] = ex.Message;
                return false;
            }
        }

        private bool RunProcess()
        {
            try
            {
                bool bo;
                string output;

                List<string> outputs = _keil.CompileAll(
                    _objectPath,
                    Path.Combine(_sessionPath, _titleProject),
                    _keilBinPath,
                    _outDirRelative,
                    out output
                );

                bo = _keil.GenerateLNPFile(
                    listFile,
                    _objectPath
                );

                _keil.BuildAll(
                    Path.Combine(_sessionPath, _titleProject),
                    _keilBinPath,
                    _outDirRelative
                );

                // ViewData["OutputCompile"] = output;
                // ViewData["OutputsCompile"] = outputs;

                //return RedirectToAction("Index", "Home");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}