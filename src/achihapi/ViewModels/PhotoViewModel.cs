using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public class PhotoViewModelBase
    {
        public String PhotoId { get; set; }
        public String Title { get; set; }
        public String Desp { get; set; }
        public String FileUrl { get; set; }
        public String ThumbnailFileUrl { get; set; }
        public String FileFormat { get; set; }
        public DateTime UploadedTime { get; set; }
        public String OrgFileName { get; set; }
        
        public Boolean IsOrgThumbnail { get; set; }
    
    }

    public class PhotoViewModel : PhotoViewModelBase
    {
        public List<ExifTagItem> ExifTags = new List<ExifTagItem>();
    }
}
