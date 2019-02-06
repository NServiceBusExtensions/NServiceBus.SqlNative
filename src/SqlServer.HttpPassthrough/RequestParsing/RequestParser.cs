using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NServiceBus.SqlServer.HttpPassthrough;

static class RequestParser
{
    public static async Task<PassthroughMessage> Extract(HttpRequest request, CancellationToken cancellation)
    {
        var incomingHeaders = HeaderReader.GetIncomingHeaders(request.Headers);
        var form = await request.ReadFormAsync(cancellation).ConfigureAwait(false);
        return new PassthroughMessage
        {
            Destination = incomingHeaders.Destination,
            Id = incomingHeaders.MessageId,
            CorrelationId = incomingHeaders.MessageId,
            Type = incomingHeaders.MessageType,
            Namespace = incomingHeaders.MessageNamespace,
            ClientUrl = incomingHeaders.Referrer,
            Body = GetMessageBody(form),
            Attachments = GetAttachments(form).ToList()
        };
    }

    static IEnumerable<Attachment> GetAttachments(IFormCollection form)
    {
        var attachments = new Dictionary<string, Attachment>();
        foreach (var file in form.Files)
        {
            var attachment = new Attachment
            {
                Stream = file.OpenReadStream,
                FileName = file.FileName
            };

            if (attachments.ContainsKey(attachment.FileName))
            {
                throw new Exception($"Duplicate filename: {attachment.FileName}");
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