using NServiceBus;
using NServiceBus.Features;

class DeduplicationFeature : Feature
{
    protected override void Setup(FeatureConfigurationContext context)
    {
        var readOnlySettings = context.Settings;
        var settings = readOnlySettings.Get<DeduplicationSettings>();

        var pipeline = context.Pipeline;
        var table = settings.Table;
        var connectionBuilder = settings.ConnectionBuilder;
        pipeline.Register(new SendRegistration(table, connectionBuilder, settings.CallbackAction));
        if (settings.RunCleanTask)
        {
            context.RegisterStartupTask(builder => new StartupTask(table, builder.Build<CriticalError>(), connectionBuilder));
        }
    }
}