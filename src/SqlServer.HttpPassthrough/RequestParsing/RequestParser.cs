using Microsoft.AspNetCore.Http;
using NServiceBus.SqlServer.HttpPassthrough;

static class RequestParser
{
    public static async Task<PassthroughMessage> Extract(HttpRequest request, Cancellation cancellation)
    {
        var incomingHeaders = HeaderReader.GetIncomingHeaders(request.Headers);
        var form = await request.ReadFormAsync(cancellation);
        return new(
            destination: incomingHeaders.Destination,
            id: incomingHeaders.MessageId,
            correlationId: incomingHeaders.MessageId,
            type: incomingHeaders.MessageType,
            @namespace: incomingHeaders.MessageNamespace,
            clientUrl: incomingHeaders.Referrer,
            body: GetMessageBody(form),
            attachments: GetAttachments(form).ToList()
        );
    }

    static IEnumerable<Attachment> GetAttachments(IFormCollection form)
    {
        var attachments = new Dictionary<string, Attachment>();
        foreach (var file in form.Files)
        {
            var attachment = new Attachment
            (
                stream: file.OpenReadStream,
                fileName: file.FileName
            );

            if (attachments.ContainsKey(attachment.FileName))
            {
                throw new($"Duplicate filename: {attachment.FileName}");
            }

            attachments.Add(attachment.FileName, attachment);
        }

        return attachments.Values;
    }

    static string GetMessageBody(IFormCollection form)
    {
        if (form.TryGetValue("message", out var stringValues))
        {
            return stringValues.ToString();
        }

        throw new BadRequestException("Expected form to contain a 'message' entry.");
    }
}