namespace NServiceBus.Transport.SqlServerNative
{
    public struct IncomingDelayedResult
    {
        public long? LastRowVersion { get; set; }
        public int Count { get; set; }
    }
}