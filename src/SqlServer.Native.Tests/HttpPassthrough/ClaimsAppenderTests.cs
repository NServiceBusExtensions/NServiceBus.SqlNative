﻿using System.Security.Claims;
using NServiceBus.SqlServer.HttpPassthrough;

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
        return Verify(headers);
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
        return Verify(result);
    }

    static IEnumerable<Claim> BuildClaims()
    {
        yield return new Claim(ClaimTypes.Email, "User@foo.bar");
        yield return new Claim(ClaimTypes.NameIdentifier, "User1");
        yield return new Claim(ClaimTypes.NameIdentifier, "User2");
    }
}