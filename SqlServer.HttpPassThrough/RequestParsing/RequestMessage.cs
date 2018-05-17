using System;
using System.Collections.Generic;
using System.Linq;
using SqlHttpPassThrough;

struct RequestMessage
{
    public Guid Id;
    public string Type;
    public string Namespace;
    public string Body;
    public List<Attachment> Attachments;
    public string Endpoint;
    public string ClientUrl;
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            {"Id", Id},
            {"Endpoint", Endpoint},
            {"Type", Type},
            {"Body", Body},
            {"Attachments", Attachments.Select(x=>x.FileName).ToList()},
        };
    }
}