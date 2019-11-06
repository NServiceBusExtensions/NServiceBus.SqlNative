using System;
using System.Diagnostics;
using System.IO;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, Expires = {Expires}")]
    public class OutgoingMessage
    {
        public OutgoingMessage(Guid id, DateTime? expires = null, string? headers = null)
        {
            Guard.AgainstEmpty(id, nameof(id));
            Guard.AgainstEmpty(headers, nameof(headers));
            Id = id;
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

        public OutgoingMessage(Guid id, DateTime? expires = null, string? headers = null, byte[]? bodyBytes = null) :
            this(id, expires, headers)
        {
            Body = bodyBytes;
        }

        public OutgoingMessage(Guid id, DateTime? expires = null, string? headers = null, Stream? bodyStream = null) :
            this(id, expires, headers)
        {
            Body = bodyStream;
        }

        public Guid Id { get; }
        public DateTime? Expires { get; }
        public string Headers { get; }
        public object? Body { get; }
    }
}