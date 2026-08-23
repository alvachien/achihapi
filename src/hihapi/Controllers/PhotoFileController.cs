using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace hihapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PhotoFileController : ControllerBase
    {
        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
        };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "image/jpeg",
            };
        }

        private static bool IsPathSafe(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            // Reject any path separators or directory traversal sequences
            if (filename.Contains("..") || filename.Contains('/') || filename.Contains('\\'))
                return false;

            // Ensure the resolved path stays within the upload folder
            var fullPath = Path.GetFullPath(Path.Combine(HIHAPIUtility.UploadFolder, filename));
            var uploadDir = Path.GetFullPath(HIHAPIUtility.UploadFolder);
            return fullPath.StartsWith(uploadDir, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reduces a user identity to a path-safe token used to tag uploaded
        /// filenames so ownership can be verified on delete without a DB table.
        /// </summary>
        private static string SanitizeUserName(string usrName)
        {
            if (string.IsNullOrWhiteSpace(usrName))
                return string.Empty;

            var sb = new System.Text.StringBuilder();
            foreach (var ch in usrName)
            {
                if (char.IsLetterOrDigit(ch))
                    sb.Append(ch);
            }
            return sb.ToString();
        }

        // GET: api/PhotoFile
        [HttpGet]
        public IActionResult Get()
        {
            return Forbid();
        }

        // GET: api/PhotoFile/filename
        // NOTE: Left [AllowAnonymous] because rendered markdown embeds these URLs as
        // <img src>, which cannot carry a Bearer token. Requiring auth here would break
        // every embedded blog/library image. The proper fix (authenticated image fetch
        // via the auth interceptor + blob URLs) is coupled to the UI auth-flow work
        // (UI-01/UI-02). The unguessable GUID filename is the current access control.
        [HttpGet("{filename}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 864000)]
        public IActionResult Get(string filename)
        {
            if (!IsPathSafe(filename))
                return BadRequest("Invalid filename");

            var fullPath = Path.Combine(HIHAPIUtility.UploadFolder, filename);
            if (System.IO.File.Exists(fullPath))
            {
                var ext = Path.GetExtension(filename);
                var contentType = GetContentType(ext);
                FileStream image = null;
                try
                {
                    image = System.IO.File.OpenRead(fullPath);
                    return File(image, contentType);
                }
                catch
                {
                    image?.Dispose();
                    throw;
                }
            }

            return NotFound();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadPhotos(ICollection<IFormFile> files)
        {
            if (files == null || files.Count <= 0)
                return BadRequest("No Files");

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

            var jsonresults = new List<PhotoFileUploadResult>();
            foreach (var file in files)
            {
                var filename1 = file.FileName;
                var idx1 = filename1.LastIndexOf('.');
                if (idx1 <= 0)
                    return BadRequest("Invalid file name: " + filename1);

                var fileext = filename1.Substring(idx1);
                if (!AllowedExtensions.Contains(fileext))
                    return BadRequest("File type not allowed: " + fileext);

                if (file.Length > MaxFileSizeBytes)
                    return BadRequest("File too large: " + filename1);

                // Tag the stored filename with the uploader so DELETE can verify ownership.
                var sanitizedUser = SanitizeUserName(usrName);
                if (string.IsNullOrEmpty(sanitizedUser))
                    throw new UnauthorizedAccessException();
                var newfilename = sanitizedUser + "_" + Guid.NewGuid().ToString("N") + fileext;

                using (var fileStream = new FileStream(Path.Combine(HIHAPIUtility.UploadFolder, newfilename), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                jsonresults.Add(new PhotoFileUploadResult
                {
                    name = filename1,
                    type = GetContentType(fileext),
                    size = (int)file.Length,
                    progress = "1.0",
                    url = "/api/PhotoFile/" + newfilename,
                    thumbnail_url = "/api/PhotoFile/" + newfilename,
                    delete_url = "/api/PhotoFile/" + newfilename,
                    delete_type = "DELETE",
                });
            }

            if (jsonresults.Count <= 0)
            {
                return Problem();
            }
            else
            {
                return new JsonResult(jsonresults.ToArray());
            }
        }

        // PUT: api/PhotoFile/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] string value)
        {
            _ = id;
            _ = value;
            return Forbid();
        }

        // DELETE: api/PhotoFile/{filename}
        [HttpDelete("{strfile}")]
        [Authorize]
        public IActionResult DeleteUploadedFile(String strfile)
        {
            if (!IsPathSafe(strfile))
                return BadRequest("Invalid filename");

            // Ownership: only the original uploader may delete a file. The uploader is
            // encoded into the stored filename at upload time (see UploadPhotos).
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
            var sanitizedUser = SanitizeUserName(usrName);
            if (string.IsNullOrEmpty(sanitizedUser) || !strfile.StartsWith(sanitizedUser + "_", StringComparison.Ordinal))
                return Forbid();

            var fileFullPath = Path.Combine(HIHAPIUtility.UploadFolder, strfile);

            try
            {
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

                return Problem("Failed to delete file.");
            }

            return Ok();
        }
    }
}
