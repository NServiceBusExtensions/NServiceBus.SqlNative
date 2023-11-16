struct IncomingHeaders(
    Guid messageId,
    string messageType,
    string? messageNamespace,
    string? destination,
    string? referrer)
{
    public Guid MessageId { get; } = messageId;
    public string MessageType { get; } = messageType;
    public string? Referrer { get; } = referrer;
    public string? Destination { get; } = destination;
    public string? MessageNamespace { get; } = messageNamespace;
}