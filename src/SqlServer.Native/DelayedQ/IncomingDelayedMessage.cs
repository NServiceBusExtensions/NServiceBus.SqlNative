namespace NServiceBus.Transport.SqlServerNative;

/// <summary>
/// Represents a message.
/// </summary>
[DebuggerDisplay("RowVersion = {RowVersion}, Due = {Due}")]
public class IncomingDelayedMessage :
    IIncomingMessage
{
    Func<ValueTask>[] cleanups;
    bool disposed;
    volatile int disposeSignaled;
    long rowVersion;
    DateTime? due;
    string headers;
    Stream? body;

    public IncomingDelayedMessage(long rowVersion, DateTime? due, string headers, Stream? body, Func<ValueTask>[] cleanups)
    {
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

        Body?.Dispose();

        disposed = true;
        foreach (var cleanup in cleanups)
        {
            await cleanup();
        }
    }
}