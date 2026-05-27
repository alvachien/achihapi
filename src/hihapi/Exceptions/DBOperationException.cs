using System;
using System.Diagnostics;

namespace hihapi.Exceptions
{
    [DebuggerDisplay("{Message}")]
    public sealed class DBOperationException : Exception
    {
        public DBOperationException(string msg) : base(msg)
        {
        }

        public DBOperationException(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }
}
