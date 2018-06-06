using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

static class ClaimsSerializer
{
    public static void Append(IEnumerable<Claim> claims, Dictionary<string, string> extraHeaders, string prefix)
    {
        foreach (var claim in claims.GroupBy(x => x.Type))
        {
            var items = claim.Select(x => x.Value).ToList();
            extraHeaders.Add(prefix + claim.Key, Serializer.SerializeList(items));
        }
    }
}