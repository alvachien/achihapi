using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TLearnEnword
    {
        public TLearnEnword()
        {
            TLearnEnwordexp = new HashSet<TLearnEnwordexp>();
        }

        public int Id { get; set; }
        public int Hid { get; set; }
        public string Word { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
        public ICollection<TLearnEnwordexp> TLearnEnwordexp { get; set; }
    }
}
