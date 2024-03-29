using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;

class HeadersBuilder(string endpoint, string machine)
{
    public string GetHeadersString(PassthroughMessage message)
    {
        var messageType = GetMessageName(message);
        var messageId = message.Id.ToString();
        var correlationId = message.CorrelationId.ToString();
        var dictionary = message.ExtraHeaders;
        dictionary.Add("NServiceBus.MessageId", messageId);
        dictionary.Add("NServiceBus.CorrelationId", correlationId);
        dictionary.Add("NServiceBus.EnclosedMessageTypes", messageType);
        dictionary.Add("NServiceBus.TimeSent", Headers.ToWireFormattedString(DateTime.UtcNow));
        dictionary.Add("NServiceBus.OriginatingMachine", machine);
        dictionary.Add("NServiceBus.OriginatingEndpoint", endpoint);
        if (message.ClientUrl != null)
        {
            dictionary.Add("MessagePassthrough.ClientUrl", message.ClientUrl);
        }

        return Headers.Serialize(dictionary);
    }

    static string GetMessageName(PassthroughMessage message)
    {
        if (message.Namespace == null)
        {
            return message.Type;
        }

        return $"{message.Namespace}.{message.Type}";
    }
}