class IncomingDelayedVerifyTarget
{
    public string Headers { get; }
    public DateTime? Due { get; }
    public string? Body { get; }

    public IncomingDelayedVerifyTarget(DateTime? due, string headers, string? body)
    {
        Due = due;
        Headers = headers;
        Body = body;
    }
}