using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Pipeline;

class DeduplicationPipelineState
{
    public bool DeduplicationOccured;

    public static bool TryGet(IOutgoingPhysicalMessageContext context, out DeduplicationPipelineState state)
    {
        return context.Extensions.TryGet(out state);
    }
    public static void Set(SendOptions options, DeduplicationPipelineState state)
    {
        options.GetExtensions().Set(state);
    }
}