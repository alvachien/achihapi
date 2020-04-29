using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace hihapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotoFileController : ControllerBase
    {
        // GET: api/PhotoFile
        [HttpGet]
        public IActionResult Get()
        {
            return Forbid();
        }

        // GET: api/PhotoFile/filename
        [HttpGet("{filename}")]
        [ResponseCache(Duration = 864000)]
        public IActionResult Get(string filename)
        {
            String strFullFile = Startup.UploadFolder + "\\" + filename;
            if (System.IO.File.Exists(strFullFile))
            {
                var image = System.IO.File.OpenRead(Startup.UploadFolder + "\\" + filename);
                return File(image, "image/jpeg");
            }

            return NotFound();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadPhotos(ICollection<IFormFile> files)
        {
            if (Request.Form.Files.Count <= 0)
                return BadRequest("No Files");

            // Only care about the first file
            var file = Request.Form.Files[0];

            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                    throw new UnauthorizedAccessException();
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            var filename1 = file.FileName;
            var idx1 = filename1.LastIndexOf('.');
            var fileext = filename1.Substring(idx1);
            var newfilename = Guid.NewGuid().ToString("N") + fileext;

            using (var fileStream = new FileStream(Path.Combine(Startup.UploadFolder, newfilename), FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Ok(filename1);
        }

        // PUT: api/PhotoFile/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return Forbid();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{strfile}")]
        [Authorize]
        public IActionResult DeleteUploadedFile(String strfile)
        {
            var fileFullPath = Path.Combine(Startup.UploadFolder, strfile);
            var filename = Path.GetFileNameWithoutExtension(fileFullPath);
            var fileext = Path.GetExtension(fileFullPath);

            try
            {
                // File
                if (System.IO.File.Exists(fileFullPath))
                {
                    System.IO.File.Delete(fileFullPath);
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif

                return BadRequest(exp.Message);
            }

            return Ok();
        }
    }
}
