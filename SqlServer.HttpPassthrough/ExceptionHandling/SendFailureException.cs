using System;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    public class SendFailureException : Exception
    {
        public PassthroughMessage PassThroughMessage { get; }

        public SendFailureException(PassthroughMessage passThroughMessage, Exception innerException) :
            base("OutgoingMessage failed to send.", innerException)
        {
            Guard.AgainstNull(passThroughMessage, nameof(passThroughMessage));
            PassThroughMessage = passThroughMessage;
        }
    }
}