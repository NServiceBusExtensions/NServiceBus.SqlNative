using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;

class SqlPassthrough : ISqlPassthrough
{
    Sender sender;
    Func<HttpContext, PassthroughMessage, Task<Table>> sendCallback;

    public SqlPassthrough(Func<HttpContext, PassthroughMessage, Task<Table>> sendCallback, Sender sender)
    {
        this.sendCallback = sendCallback;
        this.sender = sender;
    }

    public async Task Send(HttpContext context, CancellationToken cancellation = default)
    {
        Guard.AgainstNull(context, nameof(context));
        var requestMessage = await RequestParser.Extract(context.Request, cancellation).ConfigureAwait(false);
        var passThroughMessage = new PassthroughMessage
        {
            Destination = requestMessage.Destination,
            ClientUrl = requestMessage.ClientUrl,
            Type = requestMessage.Type,
            Namespace = requestMessage.Namespace,
            Id = requestMessage.Id,
            CorrelationId = requestMessage.Id,
            Attachments = requestMessage.Attachments,
            Body = requestMessage.Body
        };
        var destinationTable = await sendCallback(context, passThroughMessage).ConfigureAwait(true);
        await sender.Send(passThroughMessage, destinationTable, cancellation).ConfigureAwait(true);
    }
}