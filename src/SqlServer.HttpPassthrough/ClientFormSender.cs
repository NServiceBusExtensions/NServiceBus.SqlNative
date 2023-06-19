﻿namespace NServiceBus.SqlServer.HttpPassthrough;

/// <summary>
/// Helper class for sending passthrough messages from a client.
/// </summary>
public class ClientFormSender
{
    HttpClient client;

    /// <summary>
    /// Initializes a new instance of <see cref="ClientFormSender"/>.
    /// </summary>
    public ClientFormSender(HttpClient client) =>
        this.client = client;

    /// <summary>
    /// Send a pass through message request.
    /// </summary>
    public virtual Task<(Guid messageId, int httpStatus)> Send(string route, string message, Type messageType, Guid messageId = default, string? destination = null, Dictionary<string, byte[]>? attachments = null, Cancel cancel = default)
    {
        var typeName = messageType.Name;
        var typeNamespace = messageType.Namespace;
        return Send(route, message, typeName, messageId, typeNamespace, destination, attachments, cancel);
    }

    /// <summary>
    /// Send a pass through message request.
    /// </summary>
    public virtual async Task<(Guid messageId, int httpStatus)> Send(string route, string message, string typeName, Guid messageId = default, string? typeNamespace = null, string? destination = null, Dictionary<string, byte[]>? attachments = null, Cancel cancel = default)
    {
        Guard.AgainstNullOrEmpty(route);
        Guard.AgainstNullOrEmpty(typeName);
        Guard.AgainstNullOrEmpty(message);
        Guard.AgainstEmpty(typeNamespace);
        Guard.AgainstEmpty(destination);
        if (messageId == default)
        {
            messageId = Guid.NewGuid();
        }

        using var content = new MultipartFormDataContent
        {
            {new StringContent(message), "message"}
        };
        var headers = content.Headers;

        headers.Add("MessageType", typeName);
        if (typeNamespace != null)
        {
            headers.Add("MessageNamespace", typeNamespace);
        }

        if (messageId != default)
        {
            headers.Add("MessageId", messageId.ToString());
        }

        if (destination != null)
        {
            headers.Add("Destination", destination);
        }

        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                var file = new ByteArrayContent(attachment.Value);
                content.Add(file, attachment.Key, attachment.Key);
            }
        }

        using var response = await client.PostAsync(route, content, cancel);
        response.EnsureSuccessStatusCode();
        return (messageId, (int)response.StatusCode);
    }
}