
#if (SqlServerDeduplication)
namespace NServiceBus.Transport.SqlServerDeduplication
#else
namespace NServiceBus.Transport.SqlServerNative
#endif
{
    public enum DeduplicationOutcome
    {
        Sent,
        Deduplicated
    }
}