using System;
using System.Linq;
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
        ProcessClaims(context, passThroughMessage);
        await sender.Send(passThroughMessage, destinationTable, cancellation).ConfigureAwait(true);
    }

    void ProcessClaims(HttpContext context, PassthroughMessage passThroughMessage)
    {
        if (!appendClaims)
        {
            return;
        }

        var user = context.User;
        if (user?.Claims == null)
        {
            return;
        }

        if (!user.Claims.Any())
        {
            return;
        }

        passThroughMessage.ExtraHeaders[claimsHeaderKey] = ClaimsSerializer.Serialize(user.Claims);
    }
}