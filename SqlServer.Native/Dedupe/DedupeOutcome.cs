
#if (SqlServerDedupe)
namespace NServiceBus.Transport.SqlServerDeduplication
#else
namespace NServiceBus.Transport.SqlServerNative
#endif
{
    public enum DedupeOutcome
    {
        Sent,
        Deduplicated
    }
}