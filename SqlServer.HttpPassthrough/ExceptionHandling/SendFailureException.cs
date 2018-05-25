using System;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    /// <summary>
    /// Thrown when <see cref="ISqlPassthrough.Send"/> fails to send.
    /// </summary>
    public class SendFailureException : Exception
    {
        /// <summary>
        /// The <see cref="PassthroughMessage "/> that was attempted to send.
        /// </summary>
        public PassthroughMessage PassThroughMessage { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SendFailureException"/>
        /// </summary>
        public SendFailureException(PassthroughMessage passThroughMessage, Exception innerException) :
            base("OutgoingMessage failed to send.", innerException)
        {
            Guard.AgainstNull(passThroughMessage, nameof(passThroughMessage));
            PassThroughMessage = passThroughMessage;
        }
    }
}