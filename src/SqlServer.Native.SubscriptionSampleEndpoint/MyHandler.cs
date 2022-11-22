using SampleNamespace;

class MyHandler :
    IHandleMessages<SampleMessage>
{
    public Task Handle(SampleMessage message, IMessageHandlerContext context)
    {
        Console.WriteLine("Hello from MyHandler");
        return Task.CompletedTask;
    }
}