using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using SqlServer.Native;

static class EndpointCreator
{
    public static async Task<EndpointConfiguration> Create(string endpointName)
    {
        await QueueCreator.Create(Connection.ConnectionString, endpointName);
        var configuration = new EndpointConfiguration(endpointName);
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseSerialization<NewtonsoftSerializer>();
        configuration.DisableFeature<MessageDrivenSubscriptions>();
        return configuration;
    }
}