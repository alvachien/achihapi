using System;
using System.Diagnostics;

namespace hihapi.Exceptions
{
    [Serializable, DebuggerDisplay("{Message}")]
    public sealed class DBOperationException : InvalidOperationException 
    {
        public DBOperationException(string msg): base(msg, null)
        {
        }
    }
}
