using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class GalleryAlbumItems
    {
        public int AlbumId { get; set; }
        public int PhotoId { get; set; }

        public virtual GalleryAlbum Album { get; set; }
        public virtual GalleryItem Photo { get; set; }
    }
}
