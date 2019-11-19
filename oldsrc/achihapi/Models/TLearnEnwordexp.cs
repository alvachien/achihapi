using System;
using System.Collections.Generic;

namespace achihapi.Models
{
    public partial class TLearnEnwordexp
    {
        public int Wordid { get; set; }
        public short ExpId { get; set; }
        public string Posabb { get; set; }
        public string LangKey { get; set; }
        public string ExpDetail { get; set; }

        public TLearnEnword Word { get; set; }
    }
}
