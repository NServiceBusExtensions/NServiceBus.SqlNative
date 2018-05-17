using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using MyNamespace;
using NServiceBus;
using NServiceBus.Attachments.Sql;
using NServiceBus.Features;
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
            var manager = new DeduplicationManager(connection);
            await manager.Create();
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
        var dictionary = new Dictionary<string, string>
        {
            ["ConnectionStrings:NServiceBus"] = TestConnection.ConnectionString,
        };
        hostBuilder.ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(dictionary));
        hostBuilder.UseStartup<Startup>();
        using (var server = new TestServer(hostBuilder))
        using (var client = server.CreateClient())
        using (var content = new MultipartFormDataContent())
        {
            client.DefaultRequestHeaders.Referrer = new Uri("http://TheReferrer");
            var message = $"{{\"Property\": \"{JsonConvert.ToString(evilText)}\"}}";
            content.Add(new StringContent(message), "message");
            content.Headers.Add("MessageId", Guid.NewGuid().ToString());
            content.Headers.Add("Endpoint", "Endpoint");
            content.Headers.Add("MessageType", "MyMessage");
            content.Headers.Add("MessageNamespace", "MyNamespace");
            using (var file = new ByteArrayContent(Encoding.UTF8.GetBytes("foo")))
            {
                content.Add(file, "foofile", "foofile");

                using (var response = await client.PostAsync("/OutgoingMessage", content))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
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