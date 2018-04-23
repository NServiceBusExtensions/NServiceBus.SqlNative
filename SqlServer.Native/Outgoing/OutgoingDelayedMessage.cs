using System;

namespace SqlServer.Native
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    public class OutgoingDelayedMessage
    {
        public OutgoingDelayedMessage(DateTime due, string headers, byte[] body)
        {
            Due = due;
            Headers = headers;
            Body = body;
        }

        public DateTime Due { get; }
        public string Headers { get; }
        public byte[] Body { get; }
    }
}