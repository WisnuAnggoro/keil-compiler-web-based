using KeilCompilerWebBased.Web.Engine;
using KeilCompilerWebBased.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
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

        // Accessing all method in Keil class
        private Keil _keil;

        public ExecuteController(IConfiguration configuration)
        {
            _configuration = configuration;
            _rootPath = _configuration["RootPath"];
            _keil = new Keil();
        }

        public IActionResult Index()  
        {  
            ViewBag.connectionstring = _configuration["RootPath"];  
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