using Xunit;

public class SqlExtensionsTests
{
    [Fact]
    public void Table_name_and_schema_should_be_quoted()
    {
        Assert.Equal("[MyEndpoint]", SqlExtensions.Sanitize("MyEndpoint"));
        Assert.Equal("[MyEndpoint]]; SOME OTHER SQL;--]", SqlExtensions.Sanitize("MyEndpoint]; SOME OTHER SQL;--"));
    }
}