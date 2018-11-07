using System;
using System.Collections.Generic;
using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;

public static class EndpointMessageValidator
{
    public static Dictionary<string, HashSet<string>> Lookup = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
    {
        {
            "[dbo].[SampleEndpoint]",
            new HashSet<string>
            {
                "SampleNamespace"
            }
        }
    };

    public static void ValidateMessage(PassthroughMessage message)
    {
        var messageEndpoint = message.Destination;
        var messageNamespace = message.Namespace;
        var messageType = message.Type;
        ValidateMessage(messageEndpoint, messageNamespace, messageType);
    }

    public static void ValidateMessage(Table messageEndpoint, string messageNamespace, string messageType)
    {
        if (!Lookup.TryGetValue(messageEndpoint.FullTableName, out var messages))
        {
            throw new BadRequestException($"Not a valid endpoint. Endpoint: {messageEndpoint}");
        }

        if (!messages.Contains(messageNamespace))
        {
            throw new BadRequestException($"Not a valid namespace. Namespace: {messageNamespace}. Type: {messageType}. ");
        }
    }
}