using Microsoft.Data.SqlClient;

public class TestBase :
    IDisposable
{
    public TestBase()
    {
        SqlConnection = Connection.OpenConnection();
    }

    public SqlConnection SqlConnection;

    public virtual void Dispose()
    {
        SqlConnection?.Dispose();
    }
}
