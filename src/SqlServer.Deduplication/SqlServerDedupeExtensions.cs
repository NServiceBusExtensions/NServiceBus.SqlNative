using Microsoft.Data.SqlClient;
using NServiceBus.Configuration.AdvancedExtensibility;
using NServiceBus.Transport.SqlServerDeduplication;

namespace NServiceBus;

/// <summary>
/// Extensions to control what messages are audited.
/// </summary>
public static class SqlServerDedupeExtensions
{
    /// <summary>
    /// Enable SQL attachments for this endpoint.
    /// </summary>
    public static DedupeSettings EnableDedupe(
        this EndpointConfiguration configuration,
        Func<Cancellation, Task<SqlConnection>> connectionBuilder)
    {
        var recoverability = configuration.Recoverability();
        recoverability.AddUnrecoverableException<NotSupportedException>();
        var settings = configuration.GetSettings();
        var dedupeSettings = new DedupeSettings(connectionBuilder);
        settings.Set(dedupeSettings);
        configuration.EnableFeature<DeduplicationFeature>();
        return dedupeSettings;
    }

    public static Task<DedupeResult> SendLocalWithDedupe(this IMessageSession session, Guid messageId, object message, string? context = null, Cancellation cancellation = default)
    {
        var options = new SendOptions();
        options.RouteToThisEndpoint();
        return SendWithDedupe(session, messageId, message, options, context, cancellation);
    }

    public static Task<DedupeResult> SendWithDedupe(this IMessageSession session, Guid messageId, object message, SendOptions? options = null, string? context = null, Cancellation cancellation = default)
    {
        Guard.AgainstEmpty(messageId);
        if (options == null)
        {
            options = new();
        }
        else
        {
            ValidateMessageId(options);
        }

        return InnerSendWithDedupe(session, message, messageId, options, context, cancellation);
    }

    static void ValidateMessageId(SendOptions options)
    {
        if (options.GetMessageId() != null)
        {
            throw new ArgumentException("Expected a SendOptions with no MessageId defined", nameof(options));
        }
    }

    static async Task<DedupeResult> InnerSendWithDedupe(IMessageSession session, object message, Guid messageId, SendOptions options, string? context, Cancellation cancellation)
    {
        var pipelineState = new DedupePipelineState
        {
            Context = context
        };
        DedupePipelineState.Set(options, pipelineState);
        options.SetMessageId(messageId.ToString());

        await session.Send(message, options, cancellation);

        return new(
            dedupeOutcome: pipelineState.DedupeOutcome,
            context: pipelineState.Context
        );
    }
}