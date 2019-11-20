using NServiceBus;

namespace SampleNamespace
{
    class SampleMessage :
        IEvent
    {
        public string? Property1 { get; set; }
        public string? Property2 { get; set; }
    }
}