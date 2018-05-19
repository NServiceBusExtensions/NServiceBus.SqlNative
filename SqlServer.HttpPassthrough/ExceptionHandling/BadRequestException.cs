using System;

namespace NServiceBus.SqlServer.HttpPassThrough
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}