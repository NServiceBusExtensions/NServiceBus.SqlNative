using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Transport.SqlServerNative;
using Xunit;
using Xunit.Abstractions;
using Headers = NServiceBus.Transport.SqlServerNative.Headers;

public class SendIntegration : TestBase
{
    static ManualResetEvent resetEvent;

    [Fact]
    public async Task Run()
    {
        resetEvent = new ManualResetEvent(false);
        var configuration = await EndpointCreator.Create("IntegrationSend");
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(Connection.ConnectionString);
        configuration.DisableFeature<TimeoutManager>();
        var endpoint = await Endpoint.Start(configuration);
        await SendStartMessage();
        resetEvent.WaitOne();
        await endpoint.Stop();
    }

    Task SendStartMessage()
    {
        var sender = new QueueManager("IntegrationSend", SqlConnection);
        var headers = new Dictionary<string, string>
        {
            { "NServiceBus.EnclosedMessageTypes", typeof(SendMessage).FullName}
        };

        var message = new OutgoingMessage(Guid.NewGuid(), DateTime.Now.AddDays(1), Headers.Serialize(headers), Encoding.UTF8.GetBytes("{}"));
        return sender.Send(message);
    }

    class SendHandler : IHandleMessages<SendMessage>
    {
        public Task Handle(SendMessage message, IMessageHandlerContext context)
        {
            resetEvent.Set();
            return Task.CompletedTask;
        }
    }

    class SendMessage : IMessage
    {
    }

    public SendIntegration(ITestOutputHelper output) : base(output)
    {
    }
}