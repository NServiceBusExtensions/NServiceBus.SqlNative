using System.IO;
using NServiceBus.Transport.SqlServerNative;

static class IncomingDelayedStreamMessageHelper
{
    public static object ToVerifyTarget(this IncomingDelayedMessage result)
    {
        string readToEnd = null;
        if (result.Body != null)
        {
            using (var streamReader = new StreamReader(result.Body))
            {
                readToEnd = streamReader.ReadToEnd();
            }
        }

        return new
        {
            result.Due,
            result.Headers,
            result.RowVersion,
            bodyString = readToEnd
        };
    }
}