using NServiceBus.Transport.SqlServerNative;

public class DbSetup
{
    static bool init;
    public static void Setup()
    {
        if (init)
        {
            return;
        }

        init = true;
        SqlHelper.EnsureDatabaseExists(Connection.ConnectionString);
        using (var sqlConnection = Connection.OpenConnection())
        {
            var manager = new QueueManager("error", sqlConnection);
            manager.Create().Await();
        }
    }
}