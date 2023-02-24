using Argon;
using NServiceBus.Transport.SqlServerNative;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifySqlServer.Initialize();
        VerifierSettings.AddExtraSettings(_ => _.TypeNameHandling = TypeNameHandling.Objects);
        SqlHelper.EnsureDatabaseExists(Connection.ConnectionString);
        using var sqlConnection = Connection.OpenConnection();
        var manager = new QueueManager("error", sqlConnection);
        manager.Create().Await();
    }
}