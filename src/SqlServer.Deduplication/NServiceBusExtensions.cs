using NServiceBus.Settings;

static class NServiceBusExtensions
{
    public static bool PurgeOnStartup(this IReadOnlySettings settings)
    {
        if (settings.TryGet("Transport.PurgeOnStartup", out bool purgeOnStartup))
        {
            return purgeOnStartup;
        }
        return false;
    }
}