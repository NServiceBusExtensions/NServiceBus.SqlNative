using System;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    public class OutgoingMessage
    {
        public OutgoingMessage(Guid id, string correlationId, string replyToAddress, DateTime? expires, string headers, byte[] body)
        {
            Guard.AgainstEmpty(id, nameof(id));
            Guard.AgainstEmpty(headers, nameof(headers));
            Guard.AgainstEmpty(correlationId, nameof(correlationId));
            Guard.AgainstEmpty(replyToAddress, nameof(replyToAddress));
            Id = id;
            CorrelationId = correlationId;
            ReplyToAddress = replyToAddress;
            Expires = expires;
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

        public Guid Id { get; }
        public string CorrelationId { get; }
        public string ReplyToAddress { get; }
        public DateTime? Expires { get; }
        public string Headers { get; }
        public byte[] Body { get; }
    }
}