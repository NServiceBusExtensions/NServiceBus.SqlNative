using System.Threading.Tasks;
using NServiceBus;
using SampleNamespace;

class MyHandler : IHandleMessages<SampleMessage>
{
    public Task Handle(SampleMessage message, IMessageHandlerContext context)
    {
        return Task.CompletedTask;
    }
}