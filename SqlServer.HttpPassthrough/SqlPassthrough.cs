using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;

class SqlPassthrough : ISqlPassthrough
{
    Sender sender;
    bool appendClaims;
    string claimsHeaderKey;
    Func<HttpContext, PassthroughMessage, Task<Table>> sendCallback;

    public SqlPassthrough(Func<HttpContext, PassthroughMessage, Task<Table>> sendCallback, Sender sender, bool appendClaims, string claimsHeaderKey)
    {
        this.sendCallback = sendCallback;
        this.sender = sender;
        this.appendClaims = appendClaims;
        this.claimsHeaderKey = claimsHeaderKey;
    }

    public async Task Send(HttpContext context, CancellationToken cancellation = default)
    {
        Guard.AgainstNull(context, nameof(context));
        var passThroughMessage = await RequestParser.Extract(context.Request, cancellation).ConfigureAwait(false);
        var destinationTable = await sendCallback(context, passThroughMessage).ConfigureAwait(true);
        if (appendClaims)
        {
            passThroughMessage.ExtraHeaders[claimsHeaderKey] = ClaimsSerializer.Serialize(context.User.Claims);
        }
        await sender.Send(passThroughMessage, destinationTable, cancellation).ConfigureAwait(true);
    }
}