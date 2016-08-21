using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class AlbumViewModel
    {
        public Int32 Id { get; set; }
        [Required]
        [StringLength(50)]
        public String Title { get; set; }
        [StringLength(100)]
        public String Desp { get; set; }
        [StringLength(50)]
        public String CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Boolean IsPublic { get; set; }
        [StringLength(50)]
        public String AccessCode { get; set; }

        public Int32 PhotoCount { get; set; }
        // First photo
        public String FirstPhotoThumnailUrl { get; set; }
    }
    public class AlbumWithPhotoViewModel : AlbumViewModel
    {
        public List<PhotoViewModel> PhotoList = new List<PhotoViewModel>();
    }

}
