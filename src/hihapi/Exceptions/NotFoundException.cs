using System;
using System.Diagnostics;

namespace hihapi.Exceptions
{
    [Serializable, DebuggerDisplay("{Message}")]
    public sealed class NotFoundException : InvalidOperationException 
    {
        public NotFoundException(string msg): base(msg, null)
        {
        }
    }
}
