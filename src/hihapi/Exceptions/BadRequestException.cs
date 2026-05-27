using System;
using System.Diagnostics;

namespace hihapi.Exceptions
{
    [DebuggerDisplay("{Message}")]
    public sealed class BadRequestException : ArgumentException
    {
        public BadRequestException(string msg) : base(msg)
        {
        }

        public BadRequestException(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }
}
