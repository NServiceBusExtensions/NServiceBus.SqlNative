using System;

namespace NServiceBus.Transport.SqlServerNative
{
    public interface IIncomingMessage
    {
        string CorrelationId { get; }
        DateTime? Expires { get; }
        string Headers { get; }
        Guid Id { get; }
        long RowVersion { get; }
    }
}