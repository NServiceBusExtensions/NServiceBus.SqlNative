using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using NServiceBus.Transport.SqlServerNative;

namespace NServiceBus.SqlServer.HttpPassthrough;

/// <summary>
/// A message to be sent.
/// Used as a DTO for manipulation and verification purposes.
/// </summary>
[DebuggerDisplay("Id = {Id}, Namespace = {Namespace}, Type = {Type}, Destination = {Destination}")]
public class PassthroughMessage(
    string? destination,
    Guid id,
    Guid correlationId,
    string type,
    string? @namespace,
    string? url,
    string body,
    List<Attachment> attachments)
{
    /// <summary>
    /// The message id. Contains the 'MessageId' value from <see cref="HttpRequest.Headers"/>.
    /// </summary>
    public Guid Id
    {
        get => id;
        set
        {
            Guard.AgainstEmpty(value, nameof(Id));
            id = value;
        }
    }

    /// <summary>
    /// The correlation id. Contains the 'MessageId' value from <see cref="HttpRequest.Headers"/>.
    /// </summary>
    public Guid CorrelationId
    {
        get => correlationId;
        set
        {
            Guard.AgainstEmpty(value, nameof(CorrelationId));
            correlationId = value;
        }
    }

    /// <summary>
    /// The message type. Contains the 'MessageType' value from <see cref="HttpRequest.Headers"/>.
    /// Will be combined with <see cref="Namespace"/> and used for the 'NServiceBus.EnclosedMessageTypes' header.
    /// </summary>
    public string Type
    {
        get => type;
        set
        {
            Guard.AgainstNullOrEmpty(value, nameof(Type));
            type = value;
        }
    }

    /// <summary>
    /// The message namespace. Contains the 'MessageNamespace' value from <see cref="HttpRequest.Headers"/>.
    /// Will be combined with <see cref="Type"/> and used for the 'NServiceBus.EnclosedMessageTypes' header.
    /// </summary>
    public string? Namespace
    {
        get => @namespace;
        set
        {
            Guard.AgainstEmpty(value, nameof(Namespace));
            @namespace = value;
        }
    }

    /// <summary>
    /// The message contents. Contains the 'Message' value from the <see cref="IFormCollection"/>.
    /// </summary>
    public string Body
    {
        get => body;
        set
        {
            Guard.AgainstNullOrEmpty(value, nameof(Body));
            body = value;
        }
    }

    /// <summary>
    /// The message destination. Contains the 'Destination' value from <see cref="HttpRequest.Headers"/>.
    /// Primarily used to convert to a <see cref="Table"/> as a return value for the passthrough callback.
    /// </summary>
    public string? Destination { get; } = destination;

    /// <summary>
    /// The URL of the submitting page. Contains the <see cref="HeaderNames.Referer"/> value from <see cref="HttpRequest.Headers"/>.
    /// Will be written to a header 'MessagePassthrough.ClientUrl' in the outgoing NServiceBus message.
    /// </summary>
    public string? ClientUrl
    {
        get => url;
        set
        {
            Guard.AgainstEmpty(value, nameof(ClientUrl));
            url = value;
        }
    }

    /// <summary>
    /// The message attachments. Contains all binaries extracted from <see cref="IFormCollection.Files"/>
    /// </summary>
    public List<Attachment> Attachments { get; set; } = attachments;

    /// <summary>
    /// Any extra headers to add to the outgoing NServiceBus message.
    /// </summary>
    public Dictionary<string, string> ExtraHeaders { get; set; } = [];

    /// <summary>
    /// Convert all properties of this instance into a <see cref="Dictionary{TKey,TValue}"/>.
    /// Useful for logging and diagnostics purposes.
    /// </summary>
    public Dictionary<string, object> ToDictionary()
    {
        var objects = new Dictionary<string, object>
        {
            {"Id", Id},
            {"CorrelationId", CorrelationId},
            {"Type", Type},
            {"Body", Body},
            {"Attachments", Attachments.Select(_ => _.FileName).ToList()},
            {"ExtraHeaders", ExtraHeaders}
        };
        if (Destination != null)
        {
            objects.Add("Destination", Destination);
        }

        if (Namespace != null)
        {
            objects.Add("Namespace", Namespace);
        }

        if (ClientUrl != null)
        {
            objects.Add("ClientUrl", ClientUrl);
        }

        return objects;
    }
}