using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using NServiceBus.SqlServer.HttpPassthrough;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class ClaimsAppenderTests :
    TestBase
{
    [Fact]
    public Task Append()
    {
        var headers = new Dictionary<string, string>();
        var claims = BuildClaims();
        ClaimsAppender.Append(claims, headers, "prefix.");
        return Verifier.Verify(headers);
    }

    [Fact]
    public Task Extract()
    {
        var headers = new Dictionary<string, string>();
        var claims = BuildClaims();
        ClaimsAppender.Append(claims, headers, "prefix.");
        var result = ClaimsAppender.Extract(headers, "prefix.")
            .Select(x => new
            {
                x.Type,
                x.Value
            });
        return Verifier.Verify(result);
    }

    static IEnumerable<Claim> BuildClaims()
    {
        yield return new Claim(ClaimTypes.Email, "User@foo.bar");
        yield return new Claim(ClaimTypes.NameIdentifier, "User1");
        yield return new Claim(ClaimTypes.NameIdentifier, "User2");
    }
}