using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

// ReSharper disable UnusedVariable

public class AppendClaimsToMessageHeaders
{
    public static void WithPrefix(IServiceCollection services)
    {
        #region AppendClaimsToMessageHeaders_WithPrefix

        var configuration = new PassthroughConfiguration(
            connectionFunc: OpenConnection,
            callback: Callback,
            dedupCriticalError: exception =>
            {
                Environment.FailFast("Dedup cleanup failure", exception);
            });
        configuration.AppendClaimsToMessageHeaders(headerPrefix: "Claim.");
        services.AddSqlHttpPassthrough(configuration);

        #endregion
    }

    public static void Default(IServiceCollection services)
    {
        #region AppendClaimsToMessageHeaders

        var configuration = new PassthroughConfiguration(
            connectionFunc: OpenConnection,
            callback: Callback,
            dedupCriticalError: exception =>
            {
                Environment.FailFast("Dedup cleanup failure", exception);
            });
        configuration.AppendClaimsToMessageHeaders();
        services.AddSqlHttpPassthrough(configuration);

        #endregion
    }

    public static void AppendClaimsToDictionary(Dictionary<string, string> headerDictionary)
    {
        #region AppendClaimsToDictionary

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, "User@foo.bar"),
            new(ClaimTypes.NameIdentifier, "User1"),
            new(ClaimTypes.NameIdentifier, "User2")
        };
        ClaimsAppender.Append(claims, headerDictionary, "prefix.");

        #endregion
    }

    public static void ExtractClaimsFromDictionary(Dictionary<string, string> headerDictionary)
    {
        #region ExtractClaimsFromDictionary

        var claimsList = ClaimsAppender.Extract(headerDictionary, "prefix.");

        #endregion
    }

    #region ClaimsRaw

    public static void Append(
        IEnumerable<Claim> claims,
        IDictionary<string, string> headers, string prefix)
    {
        foreach (var claim in claims.GroupBy(_ => _.Type))
        {
            var items = claim.Select(_ => _.Value);
            headers.Add(prefix + claim.Key, JsonConvert.SerializeObject(items));
        }
    }

    public static IEnumerable<Claim> Extract(
        IDictionary<string, string> headers,
        string prefix)
    {
        foreach (var header in headers)
        {
            var key = header.Key;
            if (!key.StartsWith(prefix))
            {
                continue;
            }

            key = key.Substring(prefix.Length, key.Length - prefix.Length);
            var list = JsonConvert.DeserializeObject<List<string>>(header.Value)!;
            foreach (var value in list)
            {
                yield return new(key, value);
            }
        }
    }

    #endregion

    static Task<Table> Callback(HttpContext httpContext, PassthroughMessage passthroughMessage) =>
        null!;

    static Task<SqlConnection> OpenConnection(Cancel cancel) =>
        null!;
}