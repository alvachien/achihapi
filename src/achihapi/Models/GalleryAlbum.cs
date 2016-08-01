using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class GalleryAlbum
    {
        public GalleryAlbum()
        {
            GalleryAlbumItems = new HashSet<GalleryAlbumItems>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }

        public virtual ICollection<GalleryAlbumItems> GalleryAlbumItems { get; set; }
    }
}
