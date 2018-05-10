using System;

namespace NServiceBus.Transport.SqlServerNative
{
    public interface IIncomingDelayedMessage
    {
        DateTime Due { get; }
        string Headers { get; }
        long RowVersion { get; }
    }
}