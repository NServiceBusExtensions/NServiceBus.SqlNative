#if DEBUG

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using My.Namespace;
using NServiceBus.Attachments.Sql;

public class HttpPassthroughIntegrationTests :
    TestBase
{
    [Fact]
    public async Task Integration()
    {
        await using (var connection = Connection.OpenConnection())
        {
            var manager = new DedupeManager(connection, "Deduplication");
            await manager.Create();
            await Installer.CreateTable(connection, "MessageAttachments");
        }

        var resetEvent = new ManualResetEvent(false);
        var endpoint = await StartEndpoint(resetEvent);

        await SubmitMultipartForm();

        if (!resetEvent.WaitOne(TimeSpan.FromSeconds(2)))
        {
            throw new("OutgoingMessage not received");
        }

        await endpoint.Stop();
    }

    static async Task SubmitMultipartForm()
    {
        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseStartup<SampleStartup>();
        using var server = new TestServer(hostBuilder);
        using var client = server.CreateClient();
        client.DefaultRequestHeaders.Referrer = new("http://TheReferrer");
        var message = "{\"Property\": \"Value\"}";
        var clientFormSender = new ClientFormSender(client);
        await clientFormSender.Send(
            route: "/SendMessage",
            message: message,
            typeName: "MyMessage",
            typeNamespace: "My.Namespace",
            destination: nameof(HttpPassthroughIntegrationTests),
            attachments: new()
            {
                {"fooFile", "foo"u8.ToArray()},
                {"default", "bar"u8.ToArray()}
            });
    }

    static async Task<IEndpointInstance> StartEndpoint(ManualResetEvent resetEvent)
    {
        var configuration = await EndpointCreator.Create(nameof(HttpPassthroughIntegrationTests));
        var attachments = configuration.EnableAttachments(Connection.OpenAsyncConnection, TimeToKeep.Default);
        configuration.RegisterComponents(_ => _.AddSingleton(resetEvent));
        attachments.UseTransportConnectivity();
        return await Endpoint.Start(configuration);
    }

    class Handler(ManualResetEvent @event) :
        IHandleMessages<MyMessage>
    {
        public async Task Handle(MyMessage message, HandlerContext context)
        {
            var incomingAttachment = context.Attachments();
            Assert.NotNull(await incomingAttachment.GetBytes("fooFile", context.CancellationToken));
            Assert.NotNull(await incomingAttachment.GetBytes(context.CancellationToken));
            Assert.Equal("Value", message.Property);
            @event.Set();
        }
    }
}

#endif