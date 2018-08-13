using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Pipeline;
using NServiceBus.Transport.SqlServerDeduplication;

class DeduplicationFeature : Feature
{
    protected override void Setup(FeatureConfigurationContext context)
    {
        var readOnlySettings = context.Settings;
        var settings = readOnlySettings.Get<DeduplicationSettings>();

        var pipeline = context.Pipeline;
        pipeline.Register(new SendRegistration(settings.Table));
        if (settings.RunCleanTask)
        {
            context.RegisterStartupTask(builder =>
            {
                return new MyStartupTask(settings.Table, builder.Build<CriticalError>(), settings.ConnectionBuilder);
            });
        }
    }
}

class SendRegistration :
    RegisterStep
{
    public SendRegistration(Table table)
        : base(
            stepId: $"{AssemblyHelper.Name}Send",
            behavior: typeof(SendBehavior),
            description: "Saves the payload into the shared location",
            factoryMethod: builder => new SendBehavior(table))
    {
    }
}

class SendBehavior :
    Behavior<IOutgoingPhysicalMessageContext>
{
    Table table;

    public SendBehavior(Table table)
    {
        this.table = table;
    }

    public override Task Invoke(IOutgoingPhysicalMessageContext context, Func<Task> next)
    {
        Debug.WriteLine(context);
        return Task.CompletedTask;
    }
}