using System;
using System.Data.SqlClient;
using Newtonsoft.Json;
using ObjectApproval;
using Xunit.Abstractions;

public class TestBase:
    XunitLoggingBase,
    IDisposable
{
    public TestBase(ITestOutputHelper output) :
        base(output)
    {
        SqlConnection = Connection.OpenConnection();
    }

    static TestBase()
    {
        SerializerBuilder.ExtraSettings = settings =>
        {
            settings.TypeNameHandling = TypeNameHandling.Objects;
        };
        DbSetup.Setup();
    }

    public SqlConnection SqlConnection;

    public override void Dispose()
    {
        SqlConnection?.Dispose();
        base.Dispose();
    }
}
