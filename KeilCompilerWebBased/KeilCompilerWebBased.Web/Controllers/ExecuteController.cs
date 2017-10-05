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
//using System.Web;

namespace KeilCompilerWebBased.Web.Controllers
{
    public class ExecuteController : Controller
    {
        private IConfiguration _configuration;

        // Root path, like '/' in linux
        private static string _rootPath;

        // Path dedicated per session
        private static string _sessionPath;

        // Path to get source project
        private static string _sourcePath;

        // Path to create the HEX file
        private static string _objectPath;

        // Containing list of *.__i files
        private static List<IncludeDirectoryPath> listFile;

        // List of available OS source
        private static List<OSType> _osTypeList;

        // Accessing all method in Keil class
        private Keil _keil;

        public ExecuteController(IConfiguration configuration)
        {
            _configuration = configuration;
            _rootPath = _configuration["RootPath"];
            _keil = new Keil();

            // Initialize the available OS source
            _osTypeList = new List<OSType>();
            _osTypeList.Add(new OSType{Id = 1, Name = "Mini OS on TG132"});
            _osTypeList.Add(new OSType{Id = 2, Name = "USIM OS on TG132"});
            _osTypeList.Add(new OSType{Id = 3, Name = "USIM OS on W9F4"});
        }

        public IActionResult Index()  
        {
            OSTypeViewModel osTypeViewModel = new OSTypeViewModel();
            osTypeViewModel.OSTypeList = _osTypeList;
            osTypeViewModel.SelectedOSType = String.Empty;

            // ViewBag.connectionstring = _configuration["RootPath"];
            if(TempData["ErrorOsSelectionMessage"] != null)
                ViewBag.ErrorOsSelection = TempData["ErrorOsSelectionMessage"].ToString();
            return View(osTypeViewModel);
        }

        [HttpPost]
        public IActionResult Index(OSTypeViewModel OsTypeViewModel)
        {
            if(OsTypeViewModel.SelectedOSType != null)
                return RedirectToAction("UploadSources", OsTypeViewModel);
            else
            {
                TempData["ErrorOsSelectionMessage"] = "Please select one of the above options!";
                return RedirectToAction("Index", "Execute");
            }
        }

        public IActionResult SelectOSChip(OSTypeViewModel OsTypeViewModel)
        {
            return View(OsTypeViewModel);
        }

        public IActionResult UploadSources(OSTypeViewModel OsTypeViewModel)
        {
            // I'm strongly confident to use Parse instead of TryParse
            int iSelectOs = Int32.Parse(OsTypeViewModel.SelectedOSType);

            // Int32.TryParse(
            //     OsTypeViewModel.SelectedOSType, 
            //     out iSelectOs);

            ViewBag.SelectedOsType = _osTypeList[iSelectOs - 1].Name;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadSources(IFormFile uvprojFile)
        {
            try
            {
                string argument = "BaseCodeList:0:BasePath";
            
                _sessionPath = Path.Combine(
                    _rootPath, 
                    Path.GetFileNameWithoutExtension(
                        Path.GetRandomFileName()));
                    
                if(!Directory.Exists(_sessionPath))
                Directory.CreateDirectory(
                    _sessionPath);

                _sourcePath = _configuration[argument];
                _objectPath = Path.Combine(
                    _sessionPath, 
                    _configuration["BaseCodeList:0:ObjPath"]);    

                // Preparing source code
                _keil.UnzipBaseCodeToDirectory(
                    _configuration["BaseCodeList:0:ZipPath"],
                    _sessionPath);
                
                listFile = new List<IncludeDirectoryPath>();
                string s = 
                    $@"{_sessionPath}/{_configuration["BaseCodeList:0:Title"]}/{_configuration["BaseCodeList:0:FileName"]}";
                listFile = _keil.UVProjectFileToIFileList(
                    s);
                
                foreach (var file in listFile)
                {
                    _keil.GenerateIFile(
                        file, 
                        _objectPath);
                }
                
                return RedirectToAction("Index", "Execute");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }
        
        public ActionResult compile()
        {
            string output;

            List<string> outputs = _keil.CompileAll(
                _objectPath,
                Path.Combine(_sessionPath, @"MiniTG132"),
                out output
            );

            _keil.GenerateLNPFile(
                listFile,
                _objectPath
            );

            _keil.BuildAll(
                Path.Combine(_sessionPath, @"MiniTG132")
            );
            
            // ViewData["OutputCompile"] = output;
            // ViewData["OutputsCompile"] = outputs;

            //return RedirectToAction("Index", "Home");
            return View();
        }
    }
}