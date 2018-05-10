using System;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    public class IncomingDelayedBytesMessage : IIncomingDelayedMessage
    {
        public IncomingDelayedBytesMessage(long rowVersion, DateTime due, string headers, byte[] body)
        {
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            RowVersion = rowVersion;
            Due = due;
            Headers = headers;
            Body = body;
        }

        public long RowVersion { get; }
        public DateTime Due { get; }
        public string Headers { get; }
        public byte[] Body { get; }
    }
}