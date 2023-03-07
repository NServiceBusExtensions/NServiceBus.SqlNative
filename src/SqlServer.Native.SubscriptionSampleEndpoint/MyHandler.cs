using SampleNamespace;

class MyHandler :
    IHandleMessages<SampleMessage>
{
    public Task Handle(SampleMessage message, HandlerContext context)
    {
        Console.WriteLine("Hello from MyHandler");
        return Task.CompletedTask;
    }
}