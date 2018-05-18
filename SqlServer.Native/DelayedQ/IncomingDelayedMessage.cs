using System;
using System.IO;
using System.Threading;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    public class IncomingDelayedMessage : IIncomingMessage
    {
        IDisposable[] cleanups;
        bool disposed;
        volatile int disposeSignaled;
        long rowVersion;
        DateTime due;
        string headers;
        Stream body;

        public IncomingDelayedMessage(long rowVersion, DateTime due, string headers, Stream body, IDisposable[] cleanups)
        {
            Guard.AgainstNull(cleanups, nameof(cleanups));
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            this.cleanups = cleanups;
            this.rowVersion = rowVersion;
            this.due = due;
            this.headers = headers;
            this.body = body;
        }

        public long RowVersion
        {
            get
            {
                ThrowIfDisposed();
                return rowVersion;
            }
        }

        public DateTime Due
        {
            get
            {
                ThrowIfDisposed();
                return due;
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
                throw new ObjectDisposedException(nameof(IncomingMessage));
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