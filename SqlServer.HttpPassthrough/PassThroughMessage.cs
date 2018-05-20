using System;
using System.Collections.Generic;
using System.Linq;
using NServiceBus.Transport.SqlServerNative;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    public class PassThroughMessage
    {
        public Guid Id { get; set; }
        public Guid CorrelationId { get; set; }
        public string Type { get; set; }
        public string Namespace { get; set; }
        public string Body { get; set; }
        public Table Destination { get; set; }
        public string ClientUrl { get; set; }
        public List<Attachment> Attachments;
        public IReadOnlyDictionary<string, string> ExtraHeaders;

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                {"Id", Id},
                {"CorrelationId", CorrelationId},
                {"Destination", Destination.FullTableName},
                {"Type", Type},
                {"Namespace", Namespace},
                {"Body", Body},
                {"ClientUrl", ClientUrl},
                {"Attachments", Attachments.Select(x=>x.FileName).ToList()},
                {"ExtraHeaders", ExtraHeaders},
            };
        }
    }
}