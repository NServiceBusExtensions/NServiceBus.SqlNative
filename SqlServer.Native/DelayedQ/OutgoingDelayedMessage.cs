using System;
using System.Diagnostics;
using System.IO;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    [DebuggerDisplay("Due = {Due}")]
    public class OutgoingDelayedMessage
    {
        public OutgoingDelayedMessage(DateTime due, string headers)
        {
            Guard.AgainstEmpty(headers, nameof(headers));
            Due = due;
            if (headers == null)
            {
                Headers = SqlServerNative.Headers.EmptyHeadersJson;
            }
            else
            {
                Headers = headers;
            }
        }

        public OutgoingDelayedMessage(DateTime due, string headers, byte[] bodyBytes)
            : this(due, headers)
        {
            Body = bodyBytes;
        }

        public OutgoingDelayedMessage(DateTime due, string headers, Stream bodyStream)
            : this(due, headers)
        {
            Body = bodyStream;
        }

        public DateTime Due { get; }
        public string Headers { get; }
        public object Body { get; }
    }
}