using System.IO;
using NServiceBus.Transport.SqlServerNative;

static class IncomingMessageHelper
{
    public static IncomingVerifyTarget ToVerifyTarget(this IncomingMessage result)
    {
        string readToEnd = null;
        if (result.Body != null)
        {
            using (var streamReader = new StreamReader(result.Body))
            {
                readToEnd = streamReader.ReadToEnd();
            }
        }

        return new IncomingVerifyTarget
        {
            Expires= result.Expires,
            Headers= result.Headers,
            Id=result.Id,
            Body = readToEnd
        };
    }
}