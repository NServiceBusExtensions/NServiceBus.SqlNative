using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

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

    public static Dictionary<string, object> ToDictionary(this ClaimsPrincipal principal)
    {
        var objects = new Dictionary<string, object>();
        if (principal == null)
        {
            return objects;
        }

        if (principal.Identity != null)
        {
            objects.Add("Name", principal.Identity.Name);
        }
        if (principal.Claims != null)
        {
            objects.Add("Claims", principal.Claims.Select(x => x.ToString()));
        }

        return objects;
    }
}