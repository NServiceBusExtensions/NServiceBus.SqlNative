using Microsoft.Extensions.DependencyInjection;
using NServiceBus.Features;

class DeduplicationFeature :
    Feature
{
    protected override void Setup(FeatureConfigurationContext context)
    {
        var readOnlySettings = context.Settings;
        var settings = readOnlySettings.Get<DedupeSettings>();

        var pipeline = context.Pipeline;
        var table = settings.Table;
        var connectionBuilder = settings.ConnectionBuilder;
        pipeline.Register(new SendRegistration(table, connectionBuilder));
        if (context.Settings.PurgeOnStartup())
        {
            context.RegisterStartupTask(_ => new PurgeTask(table, connectionBuilder));
        }
        if (settings.RunCleanTask)
        {
            context.RegisterStartupTask(provider => new CleanupTask(table, provider.GetRequiredService<CriticalError>(), connectionBuilder));
        }
    }
}