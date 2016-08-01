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
    public class GalleryAlbumController : Controller
    {
        public GalleryAlbumController(achihdbContext _context)
        {
            _dbContext = _context;
        }

        private achihdbContext _dbContext = null;

        // GET: api/galleryalbum
        [HttpGet]
        public IEnumerable<GalleryAlbumViewModel> Get()
        {
            var dbentries = from ga in _dbContext.GalleryAlbum
                            select new { ga.Id, ga.Name, ga.Comment, ga.GalleryAlbumItems.Count };

            List<GalleryAlbumViewModel> vms = new List<GalleryAlbumViewModel>();
            if (dbentries != null && dbentries.Count() > 0)
            {
                foreach(var dbentry in dbentries)
                {
                    GalleryAlbumViewModel gavm = new GalleryAlbumViewModel(dbentry.Id, dbentry.Name, dbentry.Comment, dbentry.Count);
                    vms.Add(gavm);
                }
            }
            return vms;
        }

        // GET api/galleryalbum/5
        [HttpGet("{id}", Name = "GetGalleryAlbum")]
        public IActionResult Get(int id)
        {
            var dbentries = from ga in _dbContext.GalleryAlbum
                            where ga.Id == id
                            select new { ga.Id, ga.Name, ga.Comment, ga.GalleryAlbumItems.Count };
            if (dbentries.Count() <= 0)
                return NotFound();

            return new ObjectResult(new GalleryAlbumViewModel(dbentries.ElementAt(0).Id, dbentries.ElementAt(0).Name, dbentries.ElementAt(0).Comment, dbentries.ElementAt(0).Count));
        }

        // POST api/galleryalbum
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]GalleryAlbumViewModel vm)
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
            GalleryAlbum ga = new GalleryAlbum();

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    ga.Name = vm.Name;
                    ga.Comment = vm.Comment;
                    _dbContext.GalleryAlbum.Add(ga);
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

            return CreatedAtRoute("GetGalleryAlbum", new { id = ga.Id });
        }

        // PUT api/galleryalbum/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]GalleryAlbumViewModel vm)
        {
            var db = _dbContext.GalleryAlbum.SingleOrDefault(x => x.Id == id);
            if (db == null)
            {
                return NotFound();
            }

            db.Name = vm.Name;
            db.Comment = vm.Comment;
            _dbContext.GalleryAlbum.Update(db);
            _dbContext.SaveChangesAsync().Wait();

            return new ObjectResult(vm);
        }

        // DELETE api/galleryalbum/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var dbentries = _dbContext.GalleryAlbum.Where(x => x.Id == id);
            if (dbentries == null || dbentries.Count() <= 0)
            {
                return NotFound();
            }

            _dbContext.GalleryAlbum.RemoveRange(dbentries);
            _dbContext.SaveChangesAsync().Wait();

            return Ok(id);
        }
    }
}
