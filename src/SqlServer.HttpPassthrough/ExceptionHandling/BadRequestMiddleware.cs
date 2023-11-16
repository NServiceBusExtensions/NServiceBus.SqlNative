using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NServiceBus.SqlServer.HttpPassthrough;

class BadRequestMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
{
    ILogger<BadRequestMiddleware> logger = loggerFactory.CreateLogger<BadRequestMiddleware>();

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BadRequestException exception)
        {
            logger.LogError($"{exception.Message}. Headers:{{headers}}", context.RequestStringDictionary());
            var response = context.Response;
            response.StatusCode = (int) HttpStatusCode.BadRequest;
            response.ContentType = "text/plain";
            await response.WriteAsync(exception.Message);
        }
    }
}