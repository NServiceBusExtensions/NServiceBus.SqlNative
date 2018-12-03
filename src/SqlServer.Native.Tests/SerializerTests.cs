using System.Collections.Generic;
using ObjectApproval;
using Xunit;

public class SerializerTests
{
    [Fact]
    public void Dictionary()
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
        ObjectApprover.VerifyWithJson(Serializer.DeSerializeDictionary(serialized));
    }

    [Fact]
    public void List()
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
        ObjectApprover.VerifyWithJson(Serializer.DeSerializeList(serialized));
    }
}