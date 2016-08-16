using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class Album
    {
        public int AlbumId { get; set; }
        public string Title { get; set; }
        public string Desp { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreateAt { get; set; }
        public bool? IsPublic { get; set; }
        public string AccessCode { get; set; }
    }
}
