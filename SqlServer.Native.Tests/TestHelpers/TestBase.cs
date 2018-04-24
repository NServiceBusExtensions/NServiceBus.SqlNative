using Newtonsoft.Json;
using ObjectApproval;
using Xunit.Abstractions;

public class TestBase
{
    static TestBase()
    {
        DbSetup.Setup();
        var jsonSerializer = ObjectApprover.JsonSerializer;
        jsonSerializer.DefaultValueHandling = DefaultValueHandling.Ignore;
        jsonSerializer.ContractResolver = new CustomContractResolver();
    }

    public TestBase(ITestOutputHelper output)
    {
        Output = output;
    }

    protected readonly ITestOutputHelper Output;
}