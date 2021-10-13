using NServiceBus.Transport.SqlServerNative;

static class IncomingMessageHelper
{
    public static IncomingVerifyTarget ToVerifyTarget(this IncomingMessage result)
    {
        string? readToEnd = null;
        if (result.Body != null)
        {
            using var streamReader = new StreamReader(result.Body);
            readToEnd = streamReader.ReadToEnd();
        }

        return new(
            expires: result.Expires,
            headers: result.Headers,
            id: result.Id,
            body: readToEnd
        );
    }
}