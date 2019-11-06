using System;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    /// <summary>
    /// Thrown when <see cref="ISqlPassthrough.Send"/> fails to send.
    /// </summary>
    public class SendFailureException :
        Exception
    {
        /// <summary>
        /// The <see cref="HttpPassthrough.PassthroughMessage "/> that was attempted to send.
        /// </summary>
        public PassthroughMessage PassthroughMessage { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SendFailureException"/>
        /// </summary>
        public SendFailureException(PassthroughMessage passthroughMessage, Exception innerException) :
            base("OutgoingMessage failed to send.", innerException)
        {
            Guard.AgainstNull(passthroughMessage, nameof(passthroughMessage));
            PassthroughMessage = passthroughMessage;
        }
    }
}