using KeilCompilerWebBased.Web.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using KeilCompilerWebBased.Web.Models;
using System.Web;

namespace KeilCompilerWebBased.Web.Controllers
{
    public class UploadFilesController : Controller
    {
        //private readonly IHostingEnvironment _hostingEnvironment;
        // private static string _uploadPath;
        // private static string _uvProjectFileNameTemp;
        // private static string _iFilesDirectoryNameTemp;
        // private static string _objectDirectoryTemp;
        // private static List<IncludeDirectoryPath> listFile;
        // private Keil _keil;

        public UploadFilesController(/*IHostingEnvironment hostingEnvironment*/)
        {
            //_hostingEnvironment = hostingEnvironment;
            // _uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            // _uploadPath = @"D:\CSL_OS";
            // _keil = new Keil();
        }

        // [HttpPost]
        // public async Task<IActionResult> UploadUVProjectFile(IFormFile uvprojFile)
        // {
        //     if (uvprojFile != null)
        //     {
        //         _uvProjectFileNameTemp = Path.Combine(
        //             _uploadPath, 
        //             Path.GetRandomFileName());
                    
        //         _iFilesDirectoryNameTemp = Path.Combine(
        //             _uploadPath, 
        //             Path.GetFileNameWithoutExtension(            
        //                 _uvProjectFileNameTemp));        

        //         // Preparing source code
        //         _keil.UnzipBaseCodeToDirectory(
        //             _iFilesDirectoryNameTemp,
        //             out _objectDirectoryTemp);
                
        //         try
        //         {
        //             if (uvprojFile.ContentType.Equals("application/xml") || 
        //                 uvprojFile.ContentType.Equals("text/xml") || 
        //                 uvprojFile.ContentType.Equals("application/octet-stream"))
        //             {
        //                 using (var fileStream = new FileStream(
        //                     _uvProjectFileNameTemp, 
        //                     FileMode.Create))
        //                 {
        //                     await uvprojFile.CopyToAsync(fileStream);
        //                 }

        //                 listFile = new List<IncludeDirectoryPath>();
        //                 listFile = _keil.UVProjectFileToIFileList(
        //                     _uvProjectFileNameTemp);

        //                 if(!Directory.Exists(_iFilesDirectoryNameTemp))
        //                     Directory.CreateDirectory(_iFilesDirectoryNameTemp);

        //                 foreach (var file in listFile)
        //                 {
        //                     _keil.GenerateIFile(
        //                         file, 
        //                         _objectDirectoryTemp);
        //                 }

        //                 return RedirectToAction("Index", "Home");
        //             }
        //             else
        //             {
        //                 ViewBag.ErrorMessage = "The file type doesnt match";
        //                 return View("Error");
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             ViewBag.ErrorMessage = ex.Message;
        //             return View("Error");
        //         }
        //     }
        //     else
        //     {
        //         ViewBag.ErrorMessage = "There is no file to be uploaded";
        //         return View("Error");
        //     }
        // }

        // // [HttpPost]
        // // public async Task<IActionResult> UploadSourceFiles(List<IFormFile> files)
        // // {
        // //     long size = files.Sum(f => f.Length);

        // //     var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

        // //     foreach (var formFile in files)
        // //     {
        // //         if (formFile.Length > 0)
        // //         {
        // //             var filePath = Path.Combine(uploadPath, formFile.FileName).ToString();
        // //             using (var stream = new FileStream(filePath, FileMode.Create))
        // //             {
        // //                 await formFile.CopyToAsync(stream);
        // //             }
        // //         }
        // //     }

        // //     // process uploaded files
        // //     // Don't rely on or trust the FileName property without validation.

        // //     return Ok(new { count = files.Count, size, _uploadPath});
        // // }

        // public ActionResult compile()
        // {
        //     string output;

        //     _keil.CompileAll(
        //         _objectDirectoryTemp,
        //         Path.Combine(_iFilesDirectoryNameTemp, @"MiniTG132"),
        //         out output
        //     );

        //     _keil.GenerateLNPFile(
        //         listFile,
        //         _objectDirectoryTemp
        //     );

        //     _keil.BuildAll(
        //         Path.Combine(_iFilesDirectoryNameTemp, @"MiniTG132")
        //     );

        //     ViewBag.OutputCompile = "test";
        //     ViewData["OutputCompile"] = output;

        //     //return RedirectToAction("Index", "Home");
        //     return View();
        // }
    }
}