using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Transport.SqlServerNative;

static class EndpointCreator
{
    public static async Task<EndpointConfiguration> Create(string endpointName, ManualResetEvent resetEvent = null)
    {
        using (var connection = Connection.OpenConnection())
        {
            var manager = new QueueManager(endpointName, connection);
            await manager.Create();
        }

        var configuration = new EndpointConfiguration(endpointName);
        if (resetEvent != null)
        {
            configuration.RegisterComponents(x => { x.RegisterSingleton(resetEvent); });
        }

        configuration.UsePersistence<LearningPersistence>();
        configuration.UseSerialization<NewtonsoftSerializer>();
        configuration.DisableFeature<MessageDrivenSubscriptions>();
        return configuration;
    }
}