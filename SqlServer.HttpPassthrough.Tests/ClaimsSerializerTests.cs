using System.Collections.Generic;
using System.Security.Claims;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class ClaimsSerializerTests : TestBase
{
    [Fact]
    public void Simple()
    {
        var headers = new Dictionary<string, string>();
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "User@foo.bar"),
            new Claim(ClaimTypes.NameIdentifier, "User1"),
            new Claim(ClaimTypes.NameIdentifier, "User2")
        };
        ClaimsSerializer.Append(claims, headers, "prefix.");
        ObjectApprover.VerifyWithJson(headers);
    }

    public ClaimsSerializerTests(ITestOutputHelper output) : base(output)
    {
    }
}