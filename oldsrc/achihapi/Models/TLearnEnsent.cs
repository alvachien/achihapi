using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TLearnEnsent
    {
        public TLearnEnsent()
        {
            TLearnEnsentWord = new HashSet<TLearnEnsentWord>();
            TLearnEnsentexp = new HashSet<TLearnEnsentexp>();
        }

        public int Id { get; set; }
        public int Hid { get; set; }
        public string Sentence { get; set; }
        public string Createdby { get; set; }
        public DateTime? Createdat { get; set; }
        public string Updatedby { get; set; }
        public DateTime? Updatedat { get; set; }

        public THomedef H { get; set; }
        public ICollection<TLearnEnsentWord> TLearnEnsentWord { get; set; }
        public ICollection<TLearnEnsentexp> TLearnEnsentexp { get; set; }
    }
}
