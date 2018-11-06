using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using KovalukApp.Models;
namespace KovalukApp.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles ="Teacher")]
    public class FileController : Controller
    {
        IFileStorage repository;
        IHostingEnvironment environment;
        public FileController(IFileStorage repo, IHostingEnvironment env)
        {
            environment = env;
            repository = repo;
        }

        public IActionResult GetFile(string filename)
        {
            return File(repository.AllFiles.FirstOrDefault(x=>x.FileName==filename).Value, "application/octet-stream", filename);
        }

        [HttpGet]
        public IActionResult AddFile(string returnUrl)
        {
           var model= new AddFileViewModel(){ReturnUrl=returnUrl };
            return View(model);
        }
        [HttpPost]
        public IActionResult AddFile(AddFileViewModel model)
        {



            
            if (ModelState.IsValid)
            {
                if (repository.AllFiles.Any(x => x.FileName == model.File.FileName))
                {
                    ModelState.AddModelError("", "File with name " + model.File.FileName + " is alredy exist.");
                    return View(model);
                }

                var directory = Directory.GetFiles(environment.ContentRootPath + @"/wwwroot/FileExtansions");

                var ext = new String(model.File.FileName.TakeLast(model.File.FileName.Length - (model.File.FileName.LastIndexOf('.') == -1 ? model.File.FileName.Length : model.File.FileName.LastIndexOf('.')) - 1).ToArray());
                if (!directory.Any(x => x ==( "File" + ext + ".png")))
                {
                    ext = "";
                }
                var File = new AppFile() {
                    FileName =model.File.FileName,
                    FileExtansion=ext
                };
                using (var reader = new BinaryReader(model.File.OpenReadStream()))
                {
                    byte[] data = reader.ReadBytes((int)model.File.Length);
                    File.FileSize = String.Format(new FileSizeFormatProvider(), "{0:fs}",data.LongLength );
                    File.Value = data;
                }
           
                repository.AddFile(File);
                return LocalRedirect(model.ReturnUrl);
            }
            return View(model);
        }

        public IActionResult RemoveFile(string filename,string returnUrl)
        {
            var file = repository.AllFiles.FirstOrDefault(x=>x.FileName==filename);
            if(file!=null)repository.RemoveFile(file);
            return LocalRedirect(returnUrl);
        }

        public IActionResult List()
        {
            var directory = Directory.GetFiles(environment.ContentRootPath+@"/wwwroot/FileExtansions");
           
            var Model = new List<FileNameData>();
            foreach (var item in repository.AllFiles)
            {
                var Size = item.FileSize;
                    var FileName = item.FileName;
                var ext = item.FileExtansion;
                Model.Add(new FileNameData(FileName,ext,Size));
            }
            return View(Model);
        }
    }
}