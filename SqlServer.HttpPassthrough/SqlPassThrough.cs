using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;

class SqlPassThrough : ISqlPassthrough
{
    Sender sender;
    Action<HttpContext, PassthroughMessage> sendCallback;
    Func<string, Table> convertDestination;

    public SqlPassThrough(Action<HttpContext, PassthroughMessage> sendCallback, Sender sender, Func<string, Table> convertDestination)
    {
        this.sendCallback = sendCallback;
        this.sender = sender;
        this.convertDestination = convertDestination;
    }

    public async Task Send(HttpContext context, CancellationToken cancellation = default)
    {
        Guard.AgainstNull(context, nameof(context));
        var requestMessage = await RequestParser.Extract(context.Request, cancellation).ConfigureAwait(false);
        var destination = convertDestination(requestMessage.Destination);
        var passThroughMessage = new PassthroughMessage
        {
            Destination = destination,
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