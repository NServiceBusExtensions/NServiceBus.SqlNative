using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using SqlServer.Native;
using Xunit;

public class ReceiverIntegration
{
    static ReceiverIntegration()
    {
        DbSetup.Setup();
    }

    [Fact]
    public async Task Run()
    {
        await MessageQueueCreator.Drop(Connection.ConnectionString, "IntegrationReceiver_Receiver");
        await MessageQueueCreator.Create(Connection.ConnectionString, "IntegrationReceiver_Receiver");
        var configuration = await EndpointCreator.Create("IntegrationReceiver");
        configuration.SendOnly();
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(Connection.ConnectionString);
        configuration.DisableFeature<TimeoutManager>();
        var endpoint = await Endpoint.Start(configuration);
        await SendStartMessage(endpoint);
        var receiver = new Receiver();
        var message = await receiver.Receive(Connection.ConnectionString, "IntegrationReceiver_Receiver");
        Assert.NotNull(message);
    }

    static Task SendStartMessage(IEndpointInstance endpoint)
    {
        var sendOptions = new SendOptions();
        sendOptions.SetDestination("IntegrationReceiver_Receiver");
        return endpoint.Send(new SendMessage(), sendOptions);
    }

    class SendMessage : IMessage
    {
    }
}