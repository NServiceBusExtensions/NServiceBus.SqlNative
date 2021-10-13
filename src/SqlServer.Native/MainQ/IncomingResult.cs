namespace NServiceBus.Transport.SqlServerNative;

public struct IncomingResult
{
    public long? LastRowVersion { get; set; }
    public int Count { get; set; }
}