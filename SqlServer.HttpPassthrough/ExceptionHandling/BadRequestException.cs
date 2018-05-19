using System;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}