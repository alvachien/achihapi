using System;
using System.Diagnostics;

namespace hihapi.Exceptions
{
    [Serializable, DebuggerDisplay("{Message}")]
    public sealed class BadRequestException : InvalidOperationException 
    {
        public BadRequestException(string msg): base(msg, null)
        {
        }
    }
}
