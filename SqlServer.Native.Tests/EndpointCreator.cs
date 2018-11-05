using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Transport.SqlServerNative;

static class EndpointCreator
{
    public static async Task<EndpointConfiguration> Create(string endpointName)
    {
        using (var connection = Connection.OpenConnection())
        {
            var manager = new QueueManager(endpointName, connection);
            await manager.Create();
        }

        var configuration = new EndpointConfiguration(endpointName);
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(Connection.ConnectionString);
        configuration.DisableFeature<TimeoutManager>();
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseSerialization<NewtonsoftSerializer>();
        configuration.DisableFeature<MessageDrivenSubscriptions>();
        return configuration;
    }
}