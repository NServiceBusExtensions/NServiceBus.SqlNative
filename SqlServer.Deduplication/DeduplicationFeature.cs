using NServiceBus;
using NServiceBus.Features;

class DeduplicationFeature : Feature
{
    protected override void Setup(FeatureConfigurationContext context)
    {
        var readOnlySettings = context.Settings;
        var settings = readOnlySettings.Get<DeduplicationSettings>();

        var pipeline = context.Pipeline;
        pipeline.Register(new SendRegistration(settings.Table, settings.ConnectionBuilder, settings.CallbackAction));
        if (settings.RunCleanTask)
        {
            context.RegisterStartupTask(builder =>
            {
                return new StartupTask(settings.Table, builder.Build<CriticalError>(), settings.ConnectionBuilder);
            });
        }
    }
}