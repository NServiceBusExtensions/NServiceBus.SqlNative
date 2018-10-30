using System;
using System.Data.SqlClient;
using Xunit.Abstractions;

public class TestBase:IDisposable
{
    static TestBase()
    {
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
