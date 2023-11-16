using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;

class SqlPassthrough(
        Func<HttpContext, PassthroughMessage, Task<Table>> callback,
        Sender sender,
        bool claims,
        string? headerPrefix,
        ILogger logger) :
        ISqlPassthrough
{
    public async Task Send(HttpContext context, Cancel cancel = default)
    {
        var passThroughMessage = await RequestParser.Extract(context.Request, cancel);
        var destinationTable = await callback(context, passThroughMessage).ConfigureAwait(true);
        ProcessClaims(context, passThroughMessage);
        var rowVersion = await sender.Send(passThroughMessage, destinationTable, cancel).ConfigureAwait(true);
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
        if (!claims)
        {
            return;
        }

        var user = context.User;
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        if (user?.Claims == null)
        {
            return;
        }

        if (!user.Claims.Any())
        {
            return;
        }

        ClaimsAppender.Append(user.Claims, passThroughMessage.ExtraHeaders, headerPrefix);
    }
}