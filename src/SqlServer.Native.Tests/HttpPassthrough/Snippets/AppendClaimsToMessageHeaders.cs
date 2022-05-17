using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;
// ReSharper disable UnusedVariable

public class AppendClaimsToMessageHeaders
{
    public void WithPrefix(IServiceCollection services)
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

    public void Default(IServiceCollection services)
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

    public void AppendClaimsToDictionary(Dictionary<string, string> headerDictionary)
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

    public void ExtractClaimsFromDictionary(Dictionary<string, string> headerDictionary)
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
        foreach (var claim in claims.GroupBy(x => x.Type))
        {
            var items = claim.Select(x => x.Value);
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

    static Task<SqlConnection> OpenConnection(CancellationToken cancellation) =>
        null!;
}