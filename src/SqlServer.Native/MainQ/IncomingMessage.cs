namespace NServiceBus.Transport.SqlServerNative;

/// <summary>
/// Represents a message.
/// </summary>
[DebuggerDisplay("Id = {Id}, RowVersion = {RowVersion}, Expires = {Expires}")]
public class IncomingMessage :
    IIncomingMessage
{
    Func<ValueTask>[] cleanups;
    bool disposed;
    volatile int disposeSignaled;
    Guid id;
    long rowVersion;
    DateTime? expires;
    string headers;
    Stream? body;

    public IncomingMessage(Guid id, long rowVersion, DateTime? expires, string headers, Stream? body, Func<ValueTask>[] cleanups)
    {
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

        Body?.Dispose();

        disposed = true;
        foreach (var cleanup in cleanups)
        {
            await cleanup();
        }
    }
}