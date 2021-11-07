using Microsoft.AspNetCore.Http;

public class LogContextMiddleware
{
    RequestDelegate next;

    public LogContextMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            exception.Data.Add("headers", context.RequestStringDictionary());
            exception.CaptureAndThrow();
        }
    }
}