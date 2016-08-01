using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using achihapi.Models;

namespace achihapi.ViewModels
{
    public class GalleryItemViewModel
    {
        public GalleryItemViewModel()
        {
        }
        public GalleryItemViewModel(GalleryItem gi) : this()
        {
            this.Id = gi.Id;
            this.Name = gi.Name;
            this.FullUrl = gi.FullUrl;
            this.IsPublic = gi.IsPublic;
            this.FileFormat = gi.FileFormat;
            this.UploadedBy = gi.UploadedBy;
            this.UploadedDate = gi.UploadedDate;
            this.Comment = gi.Comment;
            this.CameraInfo = gi.CameraInfo;
            this.LensInfo = gi.LensInfo;
            this.Exifinfo = gi.Exifinfo;
            this.Tags = gi.Tags;
        }

        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(100)]
        public string FullUrl { get; set; }
        public bool IsPublic { get; set; }
        [StringLength(10)]
        public string FileFormat { get; set; }
        [StringLength(50)]
        public string UploadedBy { get; set; }
        public DateTime? UploadedDate { get; set; }
        [StringLength(100)]
        public string Comment { get; set; }
        [StringLength(50)]
        public string CameraInfo { get; set; }
        [StringLength(50)]
        public string LensInfo { get; set; }
        [StringLength(100)]
        public string Exifinfo { get; set; }
        [StringLength(50)]
        public string Tags { get; set; }
    }
}
