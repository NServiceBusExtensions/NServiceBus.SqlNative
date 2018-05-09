using System;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    public class IncomingBytesMessage : IIncomingMessage
    {
        public IncomingBytesMessage(Guid id, long rowVersion, DateTime? expires, string headers, byte[] body)
        {
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            Id = id;
            RowVersion = rowVersion;
            Expires = expires;
            Headers = headers;
            Body = body;
        }

        public Guid Id { get; }
        public long RowVersion { get; }
        public DateTime? Expires { get; }
        public string Headers { get; }
        public byte[] Body { get; }
    }
}