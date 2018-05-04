using System;
using System.IO;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    public class OutgoingMessage
    {
        public OutgoingMessage(Guid id, string correlationId = null, string replyToAddress = null, DateTime? expires = null, string headers = null)
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
                Headers = SqlServerNative.Headers.EmptyHeadersJson;
            }
            else
            {
                Headers = headers;
            }
        }

        public OutgoingMessage(Guid id, string correlationId = null, string replyToAddress = null, DateTime? expires = null, string headers = null, byte[] bodyBytes = null)
            : this(id, correlationId, replyToAddress, expires, headers)
        {
            BodyBytes = bodyBytes;
        }

        public OutgoingMessage(Guid id, string correlationId = null, string replyToAddress = null, DateTime? expires = null, string headers = null, Stream bodyStream = null)
            : this(id, correlationId, replyToAddress, expires, headers)
        {
            BodyStream = bodyStream;
        }

        public Guid Id { get; }
        public string CorrelationId { get; }
        public string ReplyToAddress { get; }
        public DateTime? Expires { get; }
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