using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

public class SerializerTests :
    XunitApprovalBase
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
        ObjectApprover.Verify(Serializer.DeSerializeDictionary(serialized));
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
        ObjectApprover.Verify(Serializer.DeSerializeList(serialized));
    }

    public SerializerTests(ITestOutputHelper output) :
        base(output)
    {
    }
}