using System;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    public class SendFailureException : Exception
    {
        public PassThroughMessage PassThroughMessage { get; }

        public SendFailureException(PassThroughMessage passThroughMessage, Exception innerException) :
            base("OutgoingMessage failed to send.", innerException)
        {
            Guard.AgainstNull(passThroughMessage, nameof(passThroughMessage));
            PassThroughMessage = passThroughMessage;
        }
    }
}