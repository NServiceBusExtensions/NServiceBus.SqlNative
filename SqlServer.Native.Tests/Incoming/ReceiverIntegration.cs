using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using SqlServer.Native;
using Xunit;
using Xunit.Abstractions;

public class ReceiverIntegrationTests : TestBase
{
    static string table = "IntegrationReceiver_Receiver";

    [Fact]
    public async Task Run()
    {
        await SqlHelpers.Drop(Connection.ConnectionString, table);
        await QueueCreator.Create(Connection.ConnectionString, table);
        var configuration = await EndpointCreator.Create("IntegrationReceiver");
        configuration.SendOnly();
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(Connection.ConnectionString);
        configuration.DisableFeature<TimeoutManager>();
        var endpoint = await Endpoint.Start(configuration);
        await SendStartMessage(endpoint);
        var receiver = new Receiver(table);
        var message = await receiver.Receive(Connection.ConnectionString);
        Assert.NotNull(message);
    }

    static Task SendStartMessage(IEndpointInstance endpoint)
    {
        var sendOptions = new SendOptions();
        sendOptions.SetDestination(table);
        return endpoint.Send(new SendMessage(), sendOptions);
    }

    class SendMessage : IMessage
    {
    }

    public ReceiverIntegrationTests(ITestOutputHelper output) : base(output)
    {
    }
}