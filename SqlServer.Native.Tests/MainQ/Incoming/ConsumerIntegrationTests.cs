using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Transport.SqlServerNative;
using Xunit;
using Xunit.Abstractions;

public class ConsumerIntegrationTests : TestBase
{
    static string table = "IntegrationConsumer_Consumer";

    [Fact]
    public async Task Run()
    {
        await SqlConnection.DropTable(null, table);
        var manager = new QueueManager(table, SqlConnection);
        await manager.Create();
        var configuration = await EndpointCreator.Create("IntegrationConsumer");
        configuration.SendOnly();
        var endpoint = await Endpoint.Start(configuration);
        await SendStartMessage(endpoint);
        var consumer = new QueueManager(table, SqlConnection);
        using (var message = await consumer.Consume())
        {
            Assert.NotNull(message);
        }
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

    public ConsumerIntegrationTests(ITestOutputHelper output) : base(output)
    {
    }
}