using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NServiceBus.SqlServer.HttpPassThrough;

static class RequestParser
{
    public static async Task<RequestMessage> Extract(HttpRequest request, CancellationToken cancellation)
    {
        var incomingHeaders = HeaderReader.GetIncomingHeaders(request.Headers);
        var form = await request.ReadFormAsync(cancellation).ConfigureAwait(false);
        return new RequestMessage
        {
            Destination = incomingHeaders.Destination,
            Id = incomingHeaders.MessageId,
            Type = incomingHeaders.MessageType,
            Namespace = incomingHeaders.MessageNamespace,
            ClientUrl = incomingHeaders.Referrer,
            Body = GetMessageJson(form),
            Attachments = GetAttachments(form).ToList()
        };
    }

    static IEnumerable<Attachment> GetAttachments(IFormCollection form)
    {
        return form.Files
            .Select(x =>
                new Attachment
                {
                    FileName = x.FileName,
                    Stream = x.OpenReadStream
                });
    }

    static string GetMessageJson(IFormCollection form)
    {
        if (form.TryGetValue("message", out var stringValues))
        {
            return stringValues.ToString();
        }

        throw new BadRequestException("Expected form to contain a 'message' entry.");
    }
}