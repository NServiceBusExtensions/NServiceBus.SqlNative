using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using Xunit;

public class QueueCreatorIntegration
{

    static QueueCreatorIntegration()
    {
        DbSetup.Setup();
    }

    [Fact]
    public async Task Run()
    {
        var resetEvent = new ManualResetEvent(false);
        var configuration = await EndpointCreator.Create("IntegrationSend", resetEvent);
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(Connection.ConnectionString);
        configuration.DisableFeature<TimeoutManager>();
        var endpoint = await Endpoint.Start(configuration);
        await SendStartMessage(endpoint);
        resetEvent.WaitOne();
        await endpoint.Stop();
    }

    static Task SendStartMessage(IEndpointInstance endpoint)
    {
        var sendOptions = new SendOptions();
        sendOptions.RouteToThisEndpoint();
        return endpoint.Send(new SendMessage(), sendOptions);
    }

    class SendHandler : IHandleMessages<SendMessage>
    {
        ManualResetEvent resetEvent;

        public SendHandler(ManualResetEvent resetEvent)
        {
            this.resetEvent = resetEvent;
        }

        public Task Handle(SendMessage message, IMessageHandlerContext context)
        {
            resetEvent.Set();
            return Task.CompletedTask;
        }
    }

    class SendMessage : IMessage
    {
    }
}