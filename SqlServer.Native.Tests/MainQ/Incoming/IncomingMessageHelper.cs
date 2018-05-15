using System.IO;
using NServiceBus.Transport.SqlServerNative;

static class IncomingMessageHelper
{
    public static object ToVerifyTarget(this IncomingMessage result)
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
            result.Expires,
            result.Headers,
            result.Id,
            result.RowVersion,
            bodyString = readToEnd
        };
    }
}