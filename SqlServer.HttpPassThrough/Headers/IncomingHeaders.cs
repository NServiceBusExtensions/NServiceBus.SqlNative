using System;

struct IncomingHeaders
{
    public Guid MessageId;
    public string MessageType;
    public string Referrer;
    public string Destination;
    public string MessageNamespace;
}