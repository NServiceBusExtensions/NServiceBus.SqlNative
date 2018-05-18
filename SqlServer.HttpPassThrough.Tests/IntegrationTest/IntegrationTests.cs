using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using MyNamespace;
using NServiceBus;
using NServiceBus.Attachments.Sql;
using NServiceBus.Features;
using NServiceBus.SqlServer.HttpPassThrough;
using NServiceBus.Transport.SqlServerNative;
using Xunit;
using Xunit.Abstractions;

[Trait("Category", "Integration")]
public class IntegrationTests : TestBase
{
    static ManualResetEvent resetEvent = new ManualResetEvent(false);

    [Fact]
    public async Task Integration()
    {
        using (var connection = TestConnection.OpenConnection())
        {
            var manager = new DeduplicationManager(connection, "Deduplication");
            await manager.Create();
            await Installer.CreateTable(connection,"MessageAttachments");
        }

        var endpoint = await StartEndpoint();

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
            var message = $"{{\"Property\": \"{JsonConvert.ToString(evilText)}\"}}";
            await ClientFormSender.Send(
                client,
                route: "/SendMessage",
                message: message,
                typeName: "MyMessage",
                typeNamespace: "MyNamespace",
                destination: "Endpoint", attachments: new Dictionary<string, byte[]>
                {
                    {"foofile", Encoding.UTF8.GetBytes("foo")}
                });
        }
    }

    static Task<IEndpointInstance> StartEndpoint()
    {
        var configuration = new EndpointConfiguration("Endpoint");
        configuration.UsePersistence<LearningPersistence>();
        configuration.EnableInstallers();
        configuration.PurgeOnStartup(true);
        configuration.UseSerialization<NewtonsoftSerializer>();
        configuration.DisableFeature<TimeoutManager>();
        configuration.DisableFeature<MessageDrivenSubscriptions>();
        configuration.EnableAttachments(TestConnection.ConnectionString, TimeToKeep.Default);
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(TestConnection.ConnectionString);
        return Endpoint.Start(configuration);
    }

    class Handler : IHandleMessages<MyMessage>
    {
        public async Task Handle(MyMessage message, IMessageHandlerContext context)
        {
            var incomingAttachment = context.Attachments();
            await incomingAttachment.GetBytes("foofile");
            Assert.Equal(evilText, message.Property);
            resetEvent.Set();
        }
    }

    const string evilText = @"
田中さんにあげて下さい
ヽ༼ຈل͜ຈ༽ﾉ ヽ༼ຈل͜ຈ༽ﾉ
👾 🙇 💁 🙅 🙆 🙋 🙎 🙍
 بولندا، الإطلاق عل إيو
̡͓̞ͅI̗̘̦͝n͇͇͙v̮̫ok̲̫̙͈i̖͙̭̹̠̞n̡̻̮̣̺g̲͈͙̭͙̬͎ ̰t͔̦h̞̲e̢̤ ͍̬̲͖f̴̘͕̣è͖ẹ̥̩l͖͔͚i͓͚̦͠n͖͍̗͓̳̮g͍ ̨o͚̪͡f̘̣̬ ̖̘͖̟͙̮c҉͔̫͖͓͇͖ͅh̵̤̣͚͔á̗̼͕ͅo̼̣̥s̱͈̺̖̦̻͢.̛̖̞̠̫̰

Ω≈ç√∫˜µ≤≥÷
";

    public IntegrationTests(ITestOutputHelper output) : base(output)
    {
    }
}