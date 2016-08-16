using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class Knowledge
    {
        public int Id { get; set; }
        public short? ContentType { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
