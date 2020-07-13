using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, RowVersion = {RowVersion}, Expires = {Expires}")]
    public class IncomingMessage :
        IIncomingMessage
    {
#if NETSTANDARD2_1
        IAsyncDisposable[] cleanups;
#else
        IDisposable[] cleanups;
#endif
        bool disposed;
        volatile int disposeSignaled;
        Guid id;
        long rowVersion;
        DateTime? expires;
        string headers;
        Stream? body;

#if NETSTANDARD2_1
        public IncomingMessage(Guid id, long rowVersion, DateTime? expires, string headers, Stream? body, IAsyncDisposable[] cleanups)
#else
        public IncomingMessage(Guid id, long rowVersion, DateTime? expires, string headers, Stream? body, IDisposable[] cleanups)
#endif
        {
            Guard.AgainstNull(cleanups, nameof(cleanups));
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            this.cleanups = cleanups;
            this.id = id;
            this.rowVersion = rowVersion;
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

        public Stream? Body
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

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref disposeSignaled, 1) != 0)
            {
                return;
            }

            if (Body != null)
            {
                await Body.DisposeAsync();
            }

            disposed = true;
            if (cleanups != null)
            {
                foreach (var cleanup in cleanups)
                {
                    await cleanup.DisposeAsync();
                }
            }
        }
    }
}