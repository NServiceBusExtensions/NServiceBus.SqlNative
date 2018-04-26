using System;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    public class OutgoingDelayedMessage
    {
        public OutgoingDelayedMessage(DateTime due, string headers, byte[] body)
        {
            Guard.AgainstEmpty(headers, nameof(headers));
            Due = due;
            if (headers == null)
            {
                Headers = SqlServerNative.Headers.Empty;
            }
            else
            {
                Headers = headers;
            }
            Body = body;
        }

        public DateTime Due { get; }
        public string Headers { get; }
        public byte[] Body { get; }
    }
}