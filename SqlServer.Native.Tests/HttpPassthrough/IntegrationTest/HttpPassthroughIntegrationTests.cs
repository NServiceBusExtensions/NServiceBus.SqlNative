using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using My.Namespace;
using NServiceBus;
using NServiceBus.Attachments.Sql;
using NServiceBus.Features;
using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;
using Xunit;
using Xunit.Abstractions;

public class HttpPassthroughIntegrationTests : TestBase
{
    [Fact]
    public async Task Integration()
    {
        using (var connection = Connection.OpenConnection())
        {
            var manager = new DeduplicationManager(connection, "Deduplication");
            await manager.Create();
            await Installer.CreateTable(connection, "MessageAttachments");
        }

        var resetEvent = new ManualResetEvent(false);
        var endpoint = await StartEndpoint(resetEvent);

        await SubmitMultipartForm();

        if (!resetEvent.WaitOne(TimeSpan.FromSeconds(2)))
        {
            throw new Exception("OutgoingMessage not received");
        }

        await endpoint.Stop();
    }

    static async Task SubmitMultipartForm()
    {
        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseStartup<Startup>();
        using (var server = new TestServer(hostBuilder))
        using (var client = server.CreateClient())
        {
            client.DefaultRequestHeaders.Referrer = new Uri("http://TheReferrer");
            var message = "{\"Property\": \"Value\"}";
            var clientFormSender = new ClientFormSender(client);
            await clientFormSender.Send(
                route: "/SendMessage",
                message: message,
                typeName: "MyMessage",
                typeNamespace: "My.Namespace",
                destination: "Endpoint",
                attachments: new Dictionary<string, byte[]>
                {
                    {"fooFile", Encoding.UTF8.GetBytes("foo")}
                });
        }
    }

    static Task<IEndpointInstance> StartEndpoint(ManualResetEvent resetEvent)
    {
        var configuration = new EndpointConfiguration("Endpoint");
        configuration.UsePersistence<LearningPersistence>();
        configuration.EnableInstallers();
        configuration.PurgeOnStartup(true);
        configuration.UseSerialization<NewtonsoftSerializer>();
        configuration.DisableFeature<TimeoutManager>();
        configuration.RegisterComponents(x => x.RegisterSingleton(resetEvent));
        configuration.DisableFeature<MessageDrivenSubscriptions>();
        var attachments = configuration.EnableAttachments(Connection.ConnectionString, TimeToKeep.Default);
        attachments.UseTransportConnectivity();
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(Connection.ConnectionString);
        return Endpoint.Start(configuration);
    }

    class Handler : IHandleMessages<MyMessage>
    {
        ManualResetEvent resetEvent;

        public Handler(ManualResetEvent resetEvent)
        {
            this.resetEvent = resetEvent;
        }

        public async Task Handle(MyMessage message, IMessageHandlerContext context)
        {
            var incomingAttachment = context.Attachments();
            await incomingAttachment.GetBytes("fooFile");
            Assert.Equal("Value", message.Property);
            resetEvent.Set();
        }
    }

    public HttpPassthroughIntegrationTests(ITestOutputHelper output) : base(output)
    {
    }
}