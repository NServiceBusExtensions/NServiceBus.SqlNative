using System;
using System.IO;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Represents a message.
    /// </summary>
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
            BodyBytes = bodyBytes;
        }

        public OutgoingDelayedMessage(DateTime due, string headers, Stream bodyStream)
            : this(due, headers)
        {
            BodyStream = bodyStream;
        }

        public DateTime Due { get; }
        public string Headers { get; }
        public byte[] BodyBytes { get; }
        public Stream BodyStream { get; }

        public object Body
        {
            get
            {
                if (BodyBytes == null)
                {
                    return BodyStream;
                }

                return BodyBytes;
            }
        }
    }
}