using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Attachments.Sql;
using NServiceBus.Features;
using NServiceBus.Transport.SqlServerDeduplication;
using Xunit;
using Xunit.Abstractions;

public class DedupeIntegrationTests : TestBase
{
    static CountdownEvent countdown = new CountdownEvent(2);
    static string contextResult ;

    [Fact]
    public async Task Integration()
    {
        var endpoint = await StartEndpoint();
        var messageId = Guid.NewGuid();
        await SendMessage(messageId, endpoint);
        await SendMessage(messageId, endpoint);
        if (!countdown.Wait(TimeSpan.FromSeconds(20)))
        {
            throw new Exception("Expected dedup");
        }

        await endpoint.Stop();
    }

    static async Task SendMessage(Guid messageId, IEndpointInstance endpoint)
    {
        var sendOptions = new SendOptions();
        sendOptions.RouteToThisEndpoint();
        var sendWithDedupe = await endpoint.SendWithDedupe(messageId, new MyMessage(), sendOptions);
        if (sendWithDedupe.DedupeOutcome == DedupeOutcome.Deduplicated)
        {
            contextResult = sendWithDedupe.Context;
            countdown.Signal();
        }
    }

    static Task<IEndpointInstance> StartEndpoint()
    {
        var configuration = new EndpointConfiguration(nameof(DedupeIntegrationTests));
        configuration.UsePersistence<LearningPersistence>();
        configuration.EnableInstallers();
        configuration.EnableDedupe(Connection.ConnectionString);
        configuration.PurgeOnStartup(true);
        configuration.UseSerialization<NewtonsoftSerializer>();
        configuration.DisableFeature<TimeoutManager>();
        configuration.DisableFeature<MessageDrivenSubscriptions>();

        var attachments = configuration.EnableAttachments(Connection.ConnectionString, TimeToKeep.Default);
        attachments.UseTransportConnectivity();

        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(Connection.ConnectionString);
        return Endpoint.Start(configuration);
    }

    class Handler : IHandleMessages<MyMessage>
    {
        public Task Handle(MyMessage message, IMessageHandlerContext context)
        {
            countdown.Signal();
            return Task.CompletedTask;
        }
    }

    public DedupeIntegrationTests(ITestOutputHelper output) : base(output)
    {
    }

    class MyMessage : IMessage
    {
        public string Property { get; set; }
    }
}