public class TestBase :
    IDisposable
{
    public SqlConnection SqlConnection = Connection.OpenConnection();

    public virtual void Dispose() =>
        SqlConnection.Dispose();
}
