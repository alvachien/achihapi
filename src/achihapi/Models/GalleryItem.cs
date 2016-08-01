using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class GalleryItem
    {
        public GalleryItem()
        {
            GalleryAlbumItems = new HashSet<GalleryAlbumItems>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string FullUrl { get; set; }
        public bool IsPublic { get; set; }
        public string FileFormat { get; set; }
        public string UploadedBy { get; set; }
        public DateTime? UploadedDate { get; set; }
        public string Comment { get; set; }
        public string CameraInfo { get; set; }
        public string LensInfo { get; set; }
        public string Exifinfo { get; set; }
        public string Tags { get; set; }

        public virtual ICollection<GalleryAlbumItems> GalleryAlbumItems { get; set; }
    }
}
