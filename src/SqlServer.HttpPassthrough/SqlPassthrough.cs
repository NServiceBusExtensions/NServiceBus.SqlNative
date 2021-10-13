using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;

class SqlPassthrough :
    ISqlPassthrough
{
    Sender sender;
    bool appendClaims;
    string? claimsHeaderPrefix;
    ILogger logger;
    Func<HttpContext, PassthroughMessage, Task<Table>> sendCallback;

    public SqlPassthrough(Func<HttpContext, PassthroughMessage, Task<Table>> sendCallback, Sender sender, bool appendClaims, string? claimsHeaderPrefix, ILogger logger)
    {
        this.sendCallback = sendCallback;
        this.sender = sender;
        this.appendClaims = appendClaims;
        this.claimsHeaderPrefix = claimsHeaderPrefix;
        this.logger = logger;
    }

    public async Task Send(HttpContext context, CancellationToken cancellation = default)
    {
        var passThroughMessage = await RequestParser.Extract(context.Request, cancellation);
        var destinationTable = await sendCallback(context, passThroughMessage).ConfigureAwait(true);
        ProcessClaims(context, passThroughMessage);
        var rowVersion = await sender.Send(passThroughMessage, destinationTable, cancellation).ConfigureAwait(true);
        var wasDedup = rowVersion == 0;
        if (wasDedup)
        {
            logger.LogInformation("Dedup detected. Setting response to HttpStatusCode.Conflict (409). Id:{id}", passThroughMessage.Id);
            // 208 already reported
            context.Response.StatusCode = 208;
            return;
        }
        context.Response.StatusCode = StatusCodes.Status202Accepted;
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

        ClaimsAppender.Append(user.Claims, passThroughMessage.ExtraHeaders, claimsHeaderPrefix);
    }
}