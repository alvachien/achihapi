using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using achihapi.Models;

namespace achihapi.ViewModels
{
    public class GalleryAlbumViewModel
    {
        public GalleryAlbumViewModel()
        {

        }

        public GalleryAlbumViewModel(Int32 nId, String strName, String strCmt, Int32 nCnt) : this()
        {
            this.Id = nId;
            this.Name = strName;
            this.Comment = strCmt;

            this.PhotosCount = nCnt;
        }

        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(50)]
        public string Comment { get; set; }

        // Runtime info
        public Int32 PhotosCount { get; set; }
    }
}
