using System;
using System.Diagnostics;

namespace hihapi.Exceptions
{
    [DebuggerDisplay("{Message}")]
    public sealed class UnauthorizedException : UnauthorizedAccessException
    {
        public UnauthorizedException(string msg) : base(msg)
        {
        }

        public UnauthorizedException(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }
}
