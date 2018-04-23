using System;

namespace SqlServer.Native
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    public class IncomingMessage
    {
        public IncomingMessage(Guid id, long rowVersion, string correlationId, string replyToAddress, DateTime? expires, string headers, byte[] body)
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
        public byte[] Body { get; }
    }
}