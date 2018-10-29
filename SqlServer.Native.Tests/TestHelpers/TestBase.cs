using System;
using System.Data.SqlClient;
using Newtonsoft.Json;
using ObjectApproval;
using Xunit.Abstractions;

public class TestBase:IDisposable
{
    static TestBase()
    {
        DbSetup.Setup();
        var jsonSerializer = ObjectApprover.JsonSerializer;
        jsonSerializer.ContractResolver = new CustomContractResolver();
    }

    public TestBase(ITestOutputHelper output)
    {
        Output = output;
        SqlConnection = Connection.OpenConnection();
    }

    public SqlConnection SqlConnection;

    protected readonly ITestOutputHelper Output;

    public void Dispose()
    {
        SqlConnection?.Dispose();
    }
}
