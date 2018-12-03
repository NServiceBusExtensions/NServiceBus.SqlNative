using System;

namespace NServiceBus.Transport.SqlServerNative
{
    public interface IIncomingMessage : IDisposable
    {
        long RowVersion { get; }
    }
}