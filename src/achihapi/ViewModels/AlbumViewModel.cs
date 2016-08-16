using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public class AlbumViewModel
    {
        public Int32 Id { get; set; }
        public String Title { get; set; }
        public String Desp { get; set; }
        public String CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Boolean IsPublic { get; set; }
        public String AccessCode { get; set; }

        public Int32 PhotoCount { get; set; }
        // First photo
        public String FirstPhotoThumnailUrl { get; set; }
    }
    public class AlbumWithPhotoViewModel : AlbumViewModel
    {
        public List<PhotoViewModelBase> PhotoList = new List<PhotoViewModelBase>();
    }

}
