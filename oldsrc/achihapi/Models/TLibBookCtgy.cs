using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TLibBookCtgy
    {
        public TLibBookCtgy()
        {
            TLibBook = new HashSet<TLibBook>();
        }

        public int Id { get; set; }
        public int? Hid { get; set; }
        public string Name { get; set; }
        public int? ParId { get; set; }
        public string Others { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
        public ICollection<TLibBook> TLibBook { get; set; }
    }
}
