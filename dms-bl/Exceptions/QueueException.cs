using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dms_bl.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class QueueException : Exception
    {
        public QueueException() { }

        public QueueException(string message) : base(message) { }

        public QueueException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
