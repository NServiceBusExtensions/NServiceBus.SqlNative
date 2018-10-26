using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Pipeline;
using NServiceBus.Transport.SqlServerDeduplication;

class DeduplicationPipelineState
{
    public DeduplicationOutcome DeduplicationOutcome;

    public static bool TryGet(IOutgoingPhysicalMessageContext context, out DeduplicationPipelineState state)
    {
        return context.Extensions.TryGet(out state);
    }
    public static void Set(SendOptions options, DeduplicationPipelineState state)
    {
        options.GetExtensions().Set(state);
    }
}