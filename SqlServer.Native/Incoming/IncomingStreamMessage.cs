using System;
using System.IO;
using System.Threading;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    public class IncomingStreamMessage : IIncomingMessage, IDisposable
    {
        IDisposable[] cleanups;
        bool disposed;
        volatile int disposeSignaled;
        Guid id;
        long rowVersion;
        string correlationId;
        string replyToAddress;
        DateTime? expires;
        string headers;
        Stream body;

        public IncomingStreamMessage(Guid id, long rowVersion, string correlationId, string replyToAddress, DateTime? expires, string headers, Stream body, IDisposable[] cleanups)
        {
            Guard.AgainstNull(cleanups, nameof(cleanups));
            this.cleanups = cleanups;
            this.id = id;
            this.rowVersion = rowVersion;
            this.correlationId = correlationId;
            this.replyToAddress = replyToAddress;
            this.expires = expires;
            this.headers = headers;
            this.body = body;
        }

        public Guid Id
        {
            get
            {
                ThrowIfDisposed();
                return id;
            }
        }

        public long RowVersion
        {
            get
            {
                ThrowIfDisposed();
                return rowVersion;
            }
        }

        public string CorrelationId
        {
            get
            {
                ThrowIfDisposed();
                return correlationId;
            }
        }

        public string ReplyToAddress
        {
            get
            {
                ThrowIfDisposed();
                return replyToAddress;
            }
        }

        public DateTime? Expires
        {
            get
            {
                ThrowIfDisposed();
                return expires;
            }
        }

        public string Headers
        {
            get
            {
                ThrowIfDisposed();
                return headers;
            }
        }

        public Stream Body
        {
            get
            {
                ThrowIfDisposed();
                return body;
            }
        }

        void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(IncomingStreamMessage));
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposeSignaled, 1) != 0)
            {
                return;
            }

            Body?.Dispose();
            disposed = true;
            if (cleanups != null)
            {
                foreach (var cleanup in cleanups)
                {
                    cleanup?.Dispose();
                }
            }
        }
    }
}