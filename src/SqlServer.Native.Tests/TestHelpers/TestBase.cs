using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

public class TestBase:
    XunitApprovalBase
{
    public TestBase(ITestOutputHelper output, [CallerFilePath] string sourceFilePath = "") :
        base(output, sourceFilePath)
    {
        SqlConnection = Connection.OpenConnection();
    }

    public SqlConnection SqlConnection;

    public override void Dispose()
    {
        SqlConnection?.Dispose();
        base.Dispose();
    }
}
