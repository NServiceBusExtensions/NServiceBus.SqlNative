class IncomingVerifyTarget(DateTime? expires, string headers, Guid id, string? body)
{
    public DateTime? Expires { get; } = expires;
    public string Headers { get; } = headers;
    public Guid Id { get; } = id;
    public string? Body { get; } = body;
}