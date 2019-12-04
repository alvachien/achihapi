using System;
using System.Diagnostics;

namespace hihapi.Exceptions
{
    [Serializable, DebuggerDisplay("{Message}")]
    public sealed class UnauthorizedException : InvalidOperationException 
    {
        public UnauthorizedException(string msg): base(msg, null)
        {
        }
    }
}
