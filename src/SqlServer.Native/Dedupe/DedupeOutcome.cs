
#if (SqlServerDedupe)
namespace NServiceBus.Transport.SqlServerDeduplication
#else
namespace NServiceBus.Transport.SqlServerNative
#endif
{
    public struct DedupeResult
    {
        public DedupeResult(DedupeOutcome dedupeOutcome, string? context)
        {
            DedupeOutcome = dedupeOutcome;
            Context = context;
        }

        public DedupeOutcome DedupeOutcome { get; }
        public string? Context { get; }
    }

    public enum DedupeOutcome
    {
        Sent,
        Deduplicated
    }
}