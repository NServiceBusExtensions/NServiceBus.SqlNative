using System;

namespace SqlServer.Native
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    public class Message
    {
        public Message(Guid id, string correlationId, string replyToAddress, DateTime? expires, string headers, byte[] body)
        {
            Id = id;
            CorrelationId = correlationId;
            ReplyToAddress = replyToAddress;
            Expires = expires;
            Headers = headers;
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