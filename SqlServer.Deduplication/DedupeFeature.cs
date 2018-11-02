using NServiceBus;
using NServiceBus.Features;

class DeduplicationFeature : Feature
{
    protected override void Setup(FeatureConfigurationContext context)
    {
        var readOnlySettings = context.Settings;
        var settings = readOnlySettings.Get<DedupeSettings>();

        var pipeline = context.Pipeline;
        var table = settings.Table;
        var connectionBuilder = settings.ConnectionBuilder;
        pipeline.Register(new SendRegistration(table, connectionBuilder, settings.CallbackAction));
        if (context.Settings.PurgeOnStartup())
        {
            context.RegisterStartupTask(builder => new PurgeTask(table, connectionBuilder));
        }
        if (settings.RunCleanTask)
        {
            context.RegisterStartupTask(builder => new CleanupTask(table, builder.Build<CriticalError>(), connectionBuilder));
        }
    }
}