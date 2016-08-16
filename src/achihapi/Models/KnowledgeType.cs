using System;
using System.Collections.Generic;

namespace achihapi
{
    public partial class KnowledgeType
    {
        public short Id { get; set; }
        public short? ParentId { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
    }
}
