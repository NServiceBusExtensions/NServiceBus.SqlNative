﻿using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Http;

static class Extensions
{
    public static void CaptureAndThrow(this Exception exception)
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(exception);
        dispatchInfo.Throw();
    }

    public static Dictionary<string, string> RequestStringDictionary(this HttpContext context) =>
        context.Request.Headers
            .ToDictionary(_ => _.Key, _ => _.Value.ToString());
}