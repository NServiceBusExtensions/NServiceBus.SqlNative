using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Attachments.Sql;
using NServiceBus.Features;
using Xunit;
using Xunit.Abstractions;

public class DeduplicationIntegrationTests : TestBase
{
    static CountdownEvent countdown = new CountdownEvent(2);

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

    static Task SendMessage(Guid messageId, IEndpointInstance endpoint)
    {
        var sendOptions = new SendOptions();
        sendOptions.RouteToThisEndpoint();
        return endpoint.SendWithDeduplication(messageId, new MyMessage(), sendOptions);
    }

    static Task<IEndpointInstance> StartEndpoint()
    {
        var configuration = new EndpointConfiguration(nameof(DeduplicationIntegrationTests));
        configuration.UsePersistence<LearningPersistence>();
        configuration.EnableInstallers();
        var dedup = configuration.EnableDedup(Connection.ConnectionString);
        dedup.Callback(context =>
        {
            countdown.Signal();
        });
        configuration.PurgeOnStartup(true);
        configuration.UseSerialization<NewtonsoftSerializer>();
        configuration.DisableFeature<TimeoutManager>();
        configuration.DisableFeature<MessageDrivenSubscriptions>();
        configuration.EnableAttachments(Connection.ConnectionString, TimeToKeep.Default);
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

    public DeduplicationIntegrationTests(ITestOutputHelper output) : base(output)
    {
    }

    class MyMessage : IMessage
    {
        public string Property { get; set; }
    }
}