using System;

namespace NServiceBus.Transport.SqlServerNative
{
    public interface IIncomingMessage :
        IAsyncDisposable
    {
        long RowVersion { get; }
    }
}