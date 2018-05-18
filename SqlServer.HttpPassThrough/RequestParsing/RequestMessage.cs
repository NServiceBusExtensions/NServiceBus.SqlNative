using System;
using System.Collections.Generic;
using System.Linq;
using NServiceBus.SqlServer.HttpPassThrough;

struct RequestMessage
{
    public Guid Id;
    public string Type;
    public string Namespace;
    public string Body;
    public List<Attachment> Attachments;
    public string Destination;
    public string ClientUrl;
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            {"Id", Id},
            {"Destination", Destination},
            {"Type", Type},
            {"Body", Body},
            {"Attachments", Attachments.Select(x=>x.FileName).ToList()},
        };
    }
}