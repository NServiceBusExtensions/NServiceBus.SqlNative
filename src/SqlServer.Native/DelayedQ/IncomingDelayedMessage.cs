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
    [DebuggerDisplay("RowVersion = {RowVersion}, Due = {Due}")]
    public class IncomingDelayedMessage :
        IIncomingMessage
    {
#if NETSTANDARD2_1
        IAsyncDisposable[] cleanups;
#else
        IDisposable[] cleanups;
#endif
        bool disposed;
        volatile int disposeSignaled;
        long rowVersion;
        DateTime? due;
        string headers;
        Stream? body;

#if NETSTANDARD2_1
        public IncomingDelayedMessage(long rowVersion, DateTime? due, string headers, Stream? body, IAsyncDisposable[] cleanups)
#else
        public IncomingDelayedMessage(long rowVersion, DateTime? due, string headers, Stream? body, IDisposable[] cleanups)
#endif
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

        public DateTime? Due
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
            foreach (var cleanup in cleanups)
            {
                await cleanup.DisposeAsync();
            }
        }
    }
}