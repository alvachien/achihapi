using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class Photo
    {
        public string PhotoId { get; set; }
        public string Title { get; set; }
        public string Desp { get; set; }
        public DateTime? UploadedAt { get; set; }
        public string UploadedBy { get; set; }
        public string OrgFileName { get; set; }
        public string PhotoUrl { get; set; }
        public string PhotoThumbUrl { get; set; }
        public bool? IsOrgThumb { get; set; }
        public byte? ThumbCreatedBy { get; set; }
        public string CameraMaker { get; set; }
        public string CameraModel { get; set; }
        public string LensModel { get; set; }
        public string Avnumber { get; set; }
        public string ShutterSpeed { get; set; }
        public int? Isonumber { get; set; }
        public bool? IsPublic { get; set; }
        public string Exifinfo { get; set; }
    }
}
