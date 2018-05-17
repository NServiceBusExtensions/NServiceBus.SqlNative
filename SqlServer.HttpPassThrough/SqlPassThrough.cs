using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SqlHttpPassThrough;

class SqlPassThrough: ISqlPassThrough
{
    Sender sender;
    Action<HttpContext, PassThroughMessage> sendCallback;

    public SqlPassThrough(Action<HttpContext, PassThroughMessage> sendCallback, Sender sender)
    {
        this.sendCallback = sendCallback;
        this.sender = sender;
    }

    public async Task Send(HttpContext context, CancellationToken cancellation = default)
    {
        Guard.AgainstNull(context,nameof(context));
        var requestMessage = await RequestParser.Extract(context.Request, cancellation).ConfigureAwait(false);
        var passThroughMessage = new PassThroughMessage
        {
            Endpoint = requestMessage.Endpoint,
            ClientUrl = requestMessage.ClientUrl,
            Type = requestMessage.Type,
            Namespace = requestMessage.Namespace,
            Id = requestMessage.Id,
            CorrelationId = requestMessage.Id,
            Attachments = requestMessage.Attachments,
            Body = requestMessage.Body
        };
        sendCallback(context, passThroughMessage);
        await sender.Send(passThroughMessage, cancellation).ConfigureAwait(true);
    }
}