using System;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Xunit.Abstractions;

public class TestBase:IDisposable
{
    static TestBase()
    {
        ObjectApproval.SerializerBuilder.ExtraSettings = settings => { settings.TypeNameHandling = TypeNameHandling.Objects; };
        DbSetup.Setup();
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
