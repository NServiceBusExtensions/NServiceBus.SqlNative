using System;
using System.IO;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    public class IncomingStreamMessage : IIncomingMessage
    {
        public IncomingStreamMessage(Guid id, long rowVersion, string correlationId, string replyToAddress, DateTime? expires, string headers, Stream body)
        {
            Id = id;
            RowVersion = rowVersion;
            CorrelationId = correlationId;
            ReplyToAddress = replyToAddress;
            Expires = expires;
            Headers = headers;
            Body = body;
        }

        public Guid Id { get; }
        public long RowVersion { get; }
        public string CorrelationId { get; }
        public string ReplyToAddress { get; }
        public DateTime? Expires { get; }
        public string Headers { get; }
        public Stream Body { get; }
    }

}