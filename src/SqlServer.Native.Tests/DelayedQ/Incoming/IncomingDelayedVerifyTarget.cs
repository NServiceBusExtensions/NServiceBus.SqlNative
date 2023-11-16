class IncomingDelayedVerifyTarget(DateTime? due, string headers, string? body)
{
    public string Headers { get; } = headers;
    public DateTime? Due { get; } = due;
    public string? Body { get; } = body;
}