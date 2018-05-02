using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Transport.SqlServerNative;
using Xunit;
using Xunit.Abstractions;

public class ConsumerIntegrationTests : TestBase
{
    static string table = "IntegrationConsumer_Consumer";

    [Fact]
    public async Task Run()
    {
        await SqlHelpers.Drop(SqlConnection, table);
        var manager = new QueueManager(table, SqlConnection);
        await manager.Create();
        var configuration = await EndpointCreator.Create("IntegrationConsumer");
        configuration.SendOnly();
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(Connection.ConnectionString);
        configuration.DisableFeature<TimeoutManager>();
        var endpoint = await Endpoint.Start(configuration);
        await SendStartMessage(endpoint);
        var consumer = new QueueManager(table, SqlConnection);
        var message = await consumer.ConsumeBytes();
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

    public ConsumerIntegrationTests(ITestOutputHelper output) : base(output)
    {
    }
}