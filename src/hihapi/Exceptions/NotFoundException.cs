using System;
using System.Diagnostics;

namespace hihapi.Exceptions
{
    [DebuggerDisplay("{Message}")]
    public sealed class NotFoundException : Exception
    {
        public NotFoundException(string msg) : base(msg)
        {
        }

        public NotFoundException(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }
}
