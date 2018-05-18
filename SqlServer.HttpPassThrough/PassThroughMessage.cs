﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NServiceBus.SqlServer.HttpPassThrough
{
    public class PassThroughMessage
    {
        public Guid Id { get; set; }
        public Guid CorrelationId { get; set; }
        public string Type { get; set; }
        public string Namespace { get; set; }
        public string Body { get; set; }
        public string Endpoint { get; set; }
        public string ClientUrl { get; set; }
        public List<Attachment> Attachments;
        public IReadOnlyDictionary<string, string> ExtraHeaders;

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                {"Id", Id},
                {"CorrelationId", CorrelationId},
                {"Endpoint", Endpoint},
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