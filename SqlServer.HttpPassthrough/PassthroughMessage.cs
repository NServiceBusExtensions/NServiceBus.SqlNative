using System;
using System.Collections.Generic;
using System.Linq;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    /// <summary>
    /// A message to be sent.
    /// Used as a DTO for manipulation and verification purposes when <see cref="PassthroughConfiguration.SendingCallback"/> is in use.
    /// </summary>
    public class PassthroughMessage
    {
        public Guid Id { get; set; }
        public Guid CorrelationId { get; set; }
        public string Type { get; set; }
        public string Namespace { get; set; }
        public string Body { get; set; }
        public string Destination { get; internal set; }
        public string ClientUrl { get; set; }
        public List<Attachment> Attachments { get; set; }
        public Dictionary<string, string> ExtraHeaders { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                {"Id", Id},
                {"CorrelationId", CorrelationId},
                {"Destination", Destination},
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