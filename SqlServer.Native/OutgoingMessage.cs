using System;
using System.Collections.Generic;

namespace SqlServer.Native
{
    /// <summary>
    /// Represents an outgoing message.
    /// </summary>
    public class OutgoingMessage
    {
        public OutgoingMessage( Guid id, string correlationId, string replyToAddress,TimeSpan timeToBeReceived, Dictionary<string, string> headers, byte[] body)
        {
            Guard.AgainstNull(body, nameof(body));
            Id = id;
            CorrelationId = correlationId;
            ReplyToAddress = replyToAddress;
            TimeToBeReceived = timeToBeReceived;
            if (headers != null) Headers = headers;
            Body = body;
        }

        public Guid Id { get; }
        public string CorrelationId { get; }
        public string ReplyToAddress { get; }
        public TimeSpan TimeToBeReceived { get; }
        public Dictionary<string, string> Headers { get; }
        public byte[] Body { get; }
    }
}