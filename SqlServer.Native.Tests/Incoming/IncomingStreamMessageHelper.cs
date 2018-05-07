using System.IO;
using NServiceBus.Transport.SqlServerNative;

static class IncomingStreamMessageHelper
{
    public static object ToVerifyTarget(this IncomingStreamMessage result)
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
            result.CorrelationId,
            result.Expires,
            result.Headers,
            result.Id,
            result.RowVersion,
            bodyString = readToEnd
        };
    }
}