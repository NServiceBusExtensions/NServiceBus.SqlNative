using Microsoft.Data.SqlClient;
using System.Runtime.CompilerServices;
using VerifyXunit;
using Xunit.Abstractions;

public class TestBase:
    VerifyBase
{
    public TestBase(ITestOutputHelper output,
        [CallerFilePath] string sourceFilePath = "") :
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
