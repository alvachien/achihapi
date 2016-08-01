using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using achihapi.Models;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class GalleryItemController : Controller
    {
        public GalleryItemController(achihdbContext _context)
        {
            _dbContext = _context;
        }

        private achihdbContext _dbContext = null;

        // GET: api/galleryitem
        [HttpGet]
        public IEnumerable<GalleryItemViewModel> Get()
        {
            var dbentries = from gi in _dbContext.GalleryItem
                            select new GalleryItemViewModel
                            {
                                Id = gi.Id,
                                Name = gi.Name,
                                FullUrl = gi.FullUrl,
                                IsPublic = gi.IsPublic,
                                FileFormat = gi.FileFormat,
                                UploadedBy = gi.UploadedBy,
                                UploadedDate = gi.UploadedDate,
                                Comment = gi.Comment,
                                CameraInfo = gi.CameraInfo,
                                LensInfo = gi.LensInfo,
                                Exifinfo = gi.Exifinfo,
                                Tags = gi.Tags
                            };
            if (dbentries != null && dbentries.Count() > 0)
                return dbentries.ToList();
            return new List<GalleryItemViewModel>();
        }

        // GET api/galleryitem/5
        [HttpGet("{id}", Name = "GetGalleryItem")]
        public IActionResult Get(int id)
        {
            var dbentries = from gi in _dbContext.GalleryItem
                            where gi.Id == id
                            select new GalleryItemViewModel
                            {
                                Id = gi.Id,
                                Name = gi.Name,
                                FullUrl = gi.FullUrl,
                                IsPublic = gi.IsPublic,
                                FileFormat = gi.FileFormat,
                                UploadedBy = gi.UploadedBy,
                                UploadedDate = gi.UploadedDate,
                                Comment = gi.Comment,
                                CameraInfo = gi.CameraInfo,
                                LensInfo = gi.LensInfo,
                                Exifinfo = gi.Exifinfo,
                                Tags = gi.Tags
                            };
            if (dbentries != null && dbentries.Count() > 0)
                return new ObjectResult(dbentries.ElementAt(0));

            return NotFound();
        }

        // POST api/galleryitem
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]GalleryItemViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (TryValidateModel(vm))
            {
                // Do nothing here
            }
            else
            {
                return BadRequest();
            }

            // Add it into the database
            GalleryItem ga = new GalleryItem();

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    ga.CameraInfo = vm.CameraInfo;
                    ga.Comment = vm.Comment;
                    ga.Exifinfo = vm.Exifinfo;
                    ga.FileFormat = vm.FileFormat;
                    ga.FullUrl = vm.FullUrl;
                    ga.IsPublic = vm.IsPublic;
                    ga.LensInfo = vm.LensInfo;
                    ga.Name = vm.Name;
                    ga.Tags = vm.Tags;
                    ga.UploadedBy = vm.UploadedBy;
                    ga.UploadedDate = DateTime.Now;
                    _dbContext.GalleryItem.Add(ga);
                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception exp)
                {
#if DEBUG
                    Console.WriteLine(exp.Message);
#endif

                    transaction.Rollback();
                    return BadRequest();
                }
            }

            return CreatedAtRoute("GetGalleryItem", new { id = ga.Id });
        }

        // PUT api/galleryitem/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            // Do not support
            return BadRequest();
        }

        // DELETE api/galleryitem/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var dbentries = _dbContext.GalleryItem.Where(x => x.Id == id);
            if (dbentries == null || dbentries.Count() <= 0)
            {
                return NotFound();
            }

            _dbContext.GalleryItem.RemoveRange(dbentries);
            _dbContext.SaveChangesAsync().Wait();

            return Ok(id);
        }
    }
}
