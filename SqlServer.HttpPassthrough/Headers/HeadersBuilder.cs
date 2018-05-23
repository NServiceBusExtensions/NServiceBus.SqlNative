using System;
using System.Text;
using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;

class HeadersBuilder
{
    string originatingEndpoint;
    string originatingMachine;

    public HeadersBuilder(string originatingEndpoint, string originatingMachine)
    {
        this.originatingEndpoint = originatingEndpoint;
        this.originatingMachine = originatingMachine;
    }

    public string GetHeadersString(PassthroughMessage message)
    {
        var encodedMessageType = GetEncodedMessageName(message);
        var encodedClientUrl = JsonConvert.ToString(message.ClientUrl);
        var messageId = message.Id.ToString();
        var correlationId = message.CorrelationId.ToString();
        var builder = new StringBuilder($@"{{
  ""NServiceBus.MessageId"": ""{messageId}"",
  ""NServiceBus.CorrelationId"": ""{correlationId}"",
  ""NServiceBus.EnclosedMessageTypes"": ""{encodedMessageType}"",
  ""NServiceBus.TimeSent"": ""{Headers.ToWireFormattedString(DateTime.UtcNow)}"",
  ""NServiceBus.OriginatingMachine"": ""{originatingMachine}"",
  ""NServiceBus.OriginatingEndpoint"": ""{originatingEndpoint}"",
  ""MessagePassThrough.ClientUrl"": ""{encodedClientUrl}""");
        AddExtraHeaders(message, builder);

        builder.AppendLine(@"
}");
        return builder.ToString();
    }

    static void AddExtraHeaders(PassthroughMessage message, StringBuilder builder)
    {
        if (message.ExtraHeaders == null)
        {
            return;
        }

        foreach (var header in message.ExtraHeaders)
        {
            var key = JsonConvert.ToString(header.Key);
            var value = JsonConvert.ToString(header.Value);
            builder.Append($@",
  ""{key}"": ""{value}""");
        }
    }

    public string GetEncodedMessageName(PassthroughMessage message)
    {
        if (message.Namespace == null)
        {
            return JsonConvert.ToString(message.Type);
        }

        return JsonConvert.ToString($"{message.Namespace}.{message.Type}");
    }
}