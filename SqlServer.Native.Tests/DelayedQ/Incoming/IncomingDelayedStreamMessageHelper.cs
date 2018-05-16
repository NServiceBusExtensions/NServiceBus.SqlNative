using System.IO;
using NServiceBus.Transport.SqlServerNative;

static class IncomingDelayedStreamMessageHelper
{
    public static IncomingDelayedVerifyTarget ToVerifyTarget(this IncomingDelayedMessage result)
    {
        string readToEnd = null;
        if (result.Body != null)
        {
            using (var streamReader = new StreamReader(result.Body))
            {
                readToEnd = streamReader.ReadToEnd();
            }
        }

        return new IncomingDelayedVerifyTarget
        {
            Due = result.Due,
            Headers = result.Headers,
            Body = readToEnd
        };
    }
}