using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Transport.SqlServerNative;
using Xunit;
using Xunit.Abstractions;
using Headers = NServiceBus.Transport.SqlServerNative.Headers;

public class SendIntegration :
    TestBase
{
    [Fact]
    public async Task Run()
    {
        var resetEvent = new ManualResetEvent(false);
        var configuration = await EndpointCreator.Create("IntegrationSend");
        configuration.RegisterComponents(components => components.RegisterSingleton(resetEvent));
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
            { "NServiceBus.EnclosedMessageTypes", typeof(SendMessage).FullName!}
        };

        var message = new OutgoingMessage(Guid.NewGuid(), DateTime.Now.AddDays(1), Headers.Serialize(headers), Encoding.UTF8.GetBytes("{}"));
        return sender.Send(message);
    }

    class SendHandler :
        IHandleMessages<SendMessage>
    {
        ManualResetEvent resetEvent;

        public SendHandler(ManualResetEvent resetEvent)
        {
            this.resetEvent = resetEvent;
        }

        public Task Handle(SendMessage message, IMessageHandlerContext context)
        {
            resetEvent.Set();
            return Task.CompletedTask;
        }
    }

    class SendMessage : 
        IMessage
    {
    }

    public SendIntegration(ITestOutputHelper output) :
        base(output)
    {
    }
}