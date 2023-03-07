class StreamWrapper :
    Stream
{
    Stream inner;

    public StreamWrapper(long length, Stream inner)
    {
        this.inner = inner;
        Length = length;
    }

    public override void EndWrite(IAsyncResult asyncResult) =>
        inner.EndWrite(asyncResult);

    public override void Flush() =>
        inner.Flush();

    public override Task FlushAsync(Cancellation cancellation) =>
        inner.FlushAsync(cancellation);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, Cancellation cancellation) =>
        inner.ReadAsync(buffer, offset, count, cancellation);

    public override int ReadByte() =>
        inner.ReadByte();

    public override long Seek(long offset, SeekOrigin origin) =>
        inner.Seek(offset, origin);

    public override int Read(byte[] buffer, int offset, int count) =>
        inner.Read(buffer, offset, count);

    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => inner.CanSeek;
    public override bool CanTimeout => inner.CanTimeout;
    public override bool CanWrite => false;

    public override long Length { get; }
    public override int ReadTimeout => inner.ReadTimeout;

    public override long Position
    {
        get => inner.Position;
        set => inner.Position = value;
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
        inner.BeginRead(buffer, offset, count, callback, state);

#if !NET48
    public override int Read(Span<byte> buffer) =>
        inner.Read(buffer);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, Cancellation cancellation = default) =>
        inner.ReadAsync(buffer, cancellationToken);

    public override void CopyTo(Stream destination, int bufferSize) =>
        inner.CopyTo(destination, bufferSize);

    public override void Write(ReadOnlySpan<byte> buffer) =>
        throw new NotImplementedException();

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, Cancellation cancellation = default) =>
        throw new NotImplementedException();
#endif

    public override void Close()
    {
        inner.Close();
        base.Close();
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, Cancellation cancellation) =>
        inner.CopyToAsync(destination, bufferSize, cancellation);

    public override int EndRead(IAsyncResult asyncResult) =>
        inner.EndRead(asyncResult);

#if !NET48
    [Obsolete("This Remoting API is not supported and throws PlatformNotSupportedException.", DiagnosticId = "SYSLIB0010", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
#endif
    public override object InitializeLifetimeService() =>
        inner.InitializeLifetimeService();

    public override bool Equals(object? obj) =>
        inner.Equals(obj);

    public override int GetHashCode() =>
        inner.GetHashCode();

    public override string? ToString() =>
        inner.ToString();

    public override void SetLength(long value) =>
        throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotImplementedException();

    public override Task WriteAsync(byte[] buffer, int offset, int count, Cancellation cancellation) =>
        throw new NotImplementedException();

    public override void WriteByte(byte value) =>
        throw new NotImplementedException();

    public override int WriteTimeout => throw new NotImplementedException();

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
        throw new NotImplementedException();
}