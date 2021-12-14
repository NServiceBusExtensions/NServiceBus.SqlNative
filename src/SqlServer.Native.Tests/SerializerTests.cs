[UsesVerify]
public class SerializerTests
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
        return Verifier.Verify(Serializer.DeSerializeDictionary(serialized));
    }

    [Fact]
    public Task List()
    {
        var serialized = Serializer.SerializeList(
            new()
            {
                "value",
                @"a\b",
                @"a\\b",
                "a\"b",
                "a/b",
                "a//b",
                @"a\/b"
            });
        return Verifier.Verify(Serializer.DeSerializeList(serialized));
    }
}