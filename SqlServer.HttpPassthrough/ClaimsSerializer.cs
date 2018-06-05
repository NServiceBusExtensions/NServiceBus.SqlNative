using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security.Claims;
using System.Text;

static class ClaimsSerializer
{
    static DataContractJsonSerializerSettings serializerSettings = new DataContractJsonSerializerSettings
    {
        UseSimpleDictionaryFormat = true
    };

    public static string Serialize(IEnumerable<Claim> claims)
    {
        var dictionary = new Dictionary<string, List<string>>();
        foreach (var claim in claims)
        {
            if (!dictionary.TryGetValue(claim.Type, out var list))
            {
                list = new List<string>();
                dictionary[claim.Type] = list;
            }
            list.Add(claim.Value);
        }

        var serializer = BuildSerializer();
        using (var stream = new MemoryStream())
        {
            serializer.WriteObject(stream, dictionary);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    static DataContractJsonSerializer BuildSerializer()
    {
        return new DataContractJsonSerializer(typeof(Dictionary<string, List<string>>), serializerSettings);
    }
}