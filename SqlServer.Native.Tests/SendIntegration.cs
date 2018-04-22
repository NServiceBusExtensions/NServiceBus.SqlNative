using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using SqlServer.Native;
using Xunit;

public class SendIntegration
{
    static ManualResetEvent resetEvent;

    static SendIntegration()
    {
        DbSetup.Setup();
    }

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

    static Task SendStartMessage()
    {
        var sender = new Sender();
        var headers = new Dictionary<string, string>
        {
            { "NServiceBus.EnclosedMessageTypes", typeof(SendMessage).FullName}
        };

        var message = new Message(Guid.NewGuid(), null, null, DateTime.Now.AddDays(1), HeaderSerializer.Serialize(headers), Encoding.UTF8.GetBytes("{}"));
        return sender.Send(Connection.ConnectionString, "IntegrationSend", message);
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
}