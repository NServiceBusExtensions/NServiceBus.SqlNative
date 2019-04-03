using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NServiceBus.SqlServer.HttpPassthrough;

class BadRequestMiddleware
{
    RequestDelegate next;
    ILogger<BadRequestMiddleware> logger;

    public BadRequestMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        this.next = next;
        logger = loggerFactory.CreateLogger<BadRequestMiddleware>();
    }

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