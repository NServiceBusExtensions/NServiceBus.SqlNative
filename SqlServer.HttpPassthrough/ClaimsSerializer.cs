using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Claims;
using System.Text;

static class ClaimsSerializer
{
    static DataContractJsonSerializerSettings serializerSettings = new DataContractJsonSerializerSettings
    {
        UseSimpleDictionaryFormat = true
    };

    public static void Append(IEnumerable<Claim> claims, Dictionary<string, string> extraHeaders, string prefix)
    {
        foreach (var claim in claims.GroupBy(x => x.Type))
        {
            var items = claim.Select(x => x.Value).ToList();
            extraHeaders.Add(prefix + claim.Key, Serialize(items));
        }
    }

    static string Serialize(IEnumerable<string> items)
    {
        var serializer = BuildSerializer();
        using (var stream = new MemoryStream())
        {
            serializer.WriteObject(stream, items);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    static DataContractJsonSerializer BuildSerializer()
    {
        return new DataContractJsonSerializer(typeof(List<string>), serializerSettings);
    }
}