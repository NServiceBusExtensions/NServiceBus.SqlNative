using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using My.Namespace;
using NServiceBus;
using NServiceBus.Attachments.Sql;
using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;
using Xunit;

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
                {"fooFile", Encoding.UTF8.GetBytes("foo")},
                {"default", Encoding.UTF8.GetBytes("bar")}
            });
    }

    static async Task<IEndpointInstance> StartEndpoint(ManualResetEvent resetEvent)
    {
        var configuration = await EndpointCreator.Create(nameof(HttpPassthroughIntegrationTests));
        var attachments = configuration.EnableAttachments(async () => await Connection.OpenAsyncConnection(), TimeToKeep.Default);
        configuration.RegisterComponents(components => components.RegisterSingleton(resetEvent));
        attachments.UseTransportConnectivity();
        return await Endpoint.Start(configuration);
    }

    class Handler :
        IHandleMessages<MyMessage>
    {
        ManualResetEvent resetEvent;

        public Handler(ManualResetEvent resetEvent)
        {
            this.resetEvent = resetEvent;
        }

        public async Task Handle(MyMessage message, IMessageHandlerContext context)
        {
            var incomingAttachment = context.Attachments();
            Assert.NotNull(await incomingAttachment.GetBytes("fooFile"));
            Assert.NotNull(await incomingAttachment.GetBytes());
            Assert.Equal("Value", message.Property);
            resetEvent.Set();
        }
    }
}