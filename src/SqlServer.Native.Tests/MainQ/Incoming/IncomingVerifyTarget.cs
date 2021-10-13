class IncomingVerifyTarget
{
    public DateTime? Expires { get; }
    public string Headers { get; }
    public Guid Id { get; }
    public string? Body { get; }

    public IncomingVerifyTarget(DateTime? expires, string headers, Guid id, string? body)
    {
        Expires = expires;
        Headers = headers;
        Id = id;
        Body = body;
    }
}