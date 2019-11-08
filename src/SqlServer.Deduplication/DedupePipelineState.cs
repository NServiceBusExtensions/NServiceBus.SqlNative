using System.Diagnostics.CodeAnalysis;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Pipeline;
using NServiceBus.Transport.SqlServerDeduplication;

class DedupePipelineState
{
    public DedupeOutcome DedupeOutcome;
    public string? Context;

    public static bool TryGet(
        IOutgoingPhysicalMessageContext context,
        [NotNullWhen(true)]
        out DedupePipelineState? state)
    {
        return context.Extensions.TryGet(out state);
    }

    public static void Set(SendOptions options, DedupePipelineState state)
    {
        options.GetExtensions().Set(state);
    }
}