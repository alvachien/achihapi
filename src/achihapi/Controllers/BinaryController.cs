using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Core;
using Microsoft.WindowsAzure.Storage.Shared;
using Microsoft.AspNetCore.Http;
using achihapi.Models;
using ImageMagick;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class BinaryController : Controller
    {
        private IHostingEnvironment _environment;
        private achihdbContext _dbContext = null;

        public BinaryController(IHostingEnvironment environment, achihdbContext context)
        {
            _environment = environment;
            _dbContext = context;
        }

        // GET api/knowledge/5
        [HttpGet("{id}", Name = "GetBinary")]
        public IActionResult Get(String id)
        {
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ICollection<IFormFile> files)
        {
            var uploads = Path.Combine(_environment.ContentRootPath, "wwwroot\\uploads");
            var files2 = Request.Form.Files;
            var dictmap = new Dictionary<String, String>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    String strOrgName = file.FileName;
                    var id = Guid.NewGuid();

                    GalleryItem gi = new GalleryItem();
                    gi.Name = id.ToString();
                    gi.UploadedDate = DateTime.Now;
                    gi.FileFormat = Path.GetExtension(strOrgName).TrimStart('.');
                    gi.FullUrl = "uploads/" + id.ToString() + gi.FileFormat;
                    dictmap.Add(strOrgName, gi.FullUrl);


                    using (var fileStream = new FileStream(Path.Combine(uploads, id.ToString() + gi.FileFormat), FileMode.Create))
                    {
                        using (MagickImage image = new MagickImage(fileStream))
                        {
                            // Retrieve the exif information
                            ExifProfile profile = image.GetExifProfile();

                            // Check if image contains an exif profile
                            if (profile == null)
                                Console.WriteLine("Image does not contain exif information.");
                            else
                            {
                                // Write all values to the console
                                foreach (ExifValue value in profile.Values)
                                {
                                    Console.WriteLine("{0}({1}): {2}", value.Tag, value.DataType, value.ToString());
                                }
                            }
                        }

                        var info = new MagickImageInfo(fileStream);
                        Console.WriteLine(info.Width);
                        Console.WriteLine(info.Height);
                        Console.WriteLine(info.ColorSpace);                        
                        Console.WriteLine(info.Format);
                        Console.WriteLine(info.Density.X);
                        Console.WriteLine(info.Density.Y);
                        Console.WriteLine(info.Density.Units);

                        await file.CopyToAsync(fileStream);
                    }
                }
            }

            return new ObjectResult(dictmap);
        }
    }
}
