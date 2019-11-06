using System;
using System.Net;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    /// <summary>
    /// Indicates some problem with the state of the incoming passthrough request.
    /// Can optionally be caught, but using <see cref="ConfigurationExtensions.AddSqlHttpPassthroughBadRequestMiddleware"/>,
    /// and converted to a <see cref="HttpStatusCode.BadRequest"/> to be returned to the client.
    /// </summary>
    public class BadRequestException :
        Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BadRequestException"/>.
        /// </summary>
        public BadRequestException(string message) :
            base(message)
        {
        }
    }
}