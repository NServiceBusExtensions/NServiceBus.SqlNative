using System.Data.SqlClient;
using Newtonsoft.Json;
using ObjectApproval;
using Xunit.Abstractions;

public class TestBase:
    XunitLoggingBase
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
    }

    public SqlConnection SqlConnection;

    public override void Dispose()
    {
        SqlConnection?.Dispose();
        base.Dispose();
    }
}
