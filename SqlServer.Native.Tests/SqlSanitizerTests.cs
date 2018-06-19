using Xunit;

public class SqlSanitizerTests
{
    [Fact]
    public void Table_name_and_schema_should_be_quoted()
    {
        Assert.Equal("[MyEndpoint]", SqlSanitizer.Sanitize("MyEndpoint"));
        Assert.Equal("[MyEndpoint]]; SOME OTHER SQL;--]", SqlSanitizer.Sanitize("MyEndpoint]; SOME OTHER SQL;--"));
    }
}