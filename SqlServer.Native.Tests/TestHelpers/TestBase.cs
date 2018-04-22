using Newtonsoft.Json;
using ObjectApproval;
using Xunit.Abstractions;

public class TestBase
{
    static TestBase()
    {
        var jsonSerializer = ObjectApprover.JsonSerializer;
        jsonSerializer.DefaultValueHandling = DefaultValueHandling.Ignore;
        jsonSerializer.ContractResolver = new CustomContractResolver();
        var converters = jsonSerializer.Converters;
        converters.Add(new GuidConverter());
        converters.Add(new StringConverter());
    }

    public TestBase(ITestOutputHelper output)
    {
        Output = output;
    }

    protected readonly ITestOutputHelper Output;
}