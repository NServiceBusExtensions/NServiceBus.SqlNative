using System.Runtime.ExceptionServices;

static class Extensions
{
    public static void CaptureAndThrow(this Exception exception)
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(exception);
        dispatchInfo.Throw();
    }

    public static Dictionary<string, string> RequestStringDictionary(this HttpContext context)
    {
        return context.Request.Headers.ToDictionary(x => x.Key, y => y.Value.ToString());
    }
}