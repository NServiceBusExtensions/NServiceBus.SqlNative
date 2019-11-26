using System.Collections.Generic;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class SerializerTests :
    VerifyBase
{
    [Fact]
    public Task Dictionary()
    {
        var serialized = Serializer.SerializeDictionary(
            new Dictionary<string, string>
            {
                {"key", "value"},
                {@"a\b", @"a\b"},
                {@"a\\b", @"a\\b"},
                {"a\"b", "a\"b"},
                {"a/b", "a/b"},
                {"a//b", "a//b"},
                {@"a\/b", @"a\/b"}
            });
        return Verify(Serializer.DeSerializeDictionary(serialized));
    }

    [Fact]
    public Task List()
    {
        var serialized = Serializer.SerializeList(
            new List<string>
            {
                "value",
                @"a\b",
                @"a\\b",
                "a\"b",
                "a/b",
                "a//b",
                @"a\/b"
            });
        return Verify(Serializer.DeSerializeList(serialized));
    }

    public SerializerTests(ITestOutputHelper output) :
        base(output)
    {
    }
}