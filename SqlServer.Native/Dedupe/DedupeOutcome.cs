
#if (SqlServerDedupe)
namespace NServiceBus.Transport.SqlServerDeduplication
#else
namespace NServiceBus.Transport.SqlServerNative
#endif
{
    public struct DedupeResult
    {
        public DedupeOutcome DedupeOutcome { get; set; }
        public string Context { get; set; }
    }

    public enum DedupeOutcome
    {
        Sent,
        Deduplicated
    }
}