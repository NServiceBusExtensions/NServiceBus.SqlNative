struct IncomingHeaders
{
    public Guid MessageId { get; }
    public string MessageType { get; }
    public string? Referrer { get; }
    public string? Destination { get; }
    public string? MessageNamespace { get; }

    public IncomingHeaders(Guid messageId, string messageType, string? messageNamespace, string? destination, string? referrer)
    {
        MessageId = messageId;
        MessageType = messageType;
        MessageNamespace = messageNamespace;
        Destination = destination;
        Referrer = referrer;
    }
}