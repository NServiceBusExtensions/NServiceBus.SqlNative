using Newtonsoft.Json;
using NServiceBus.Transport.SqlServerNative;
using VerifyTests;

public static class ModuleInitializer
{
    public static void Initialize()
    {
        VerifySqlServer.Enable();
        VerifierSettings.ModifySerialization(settings =>
        {
            settings.AddExtraSettings(serializerSettings =>
                serializerSettings.TypeNameHandling = TypeNameHandling.Objects);
        });
        SqlHelper.EnsureDatabaseExists(Connection.ConnectionString);
        using var sqlConnection = Connection.OpenConnection();
        var manager = new QueueManager("error", sqlConnection);
        manager.Create().Await();
    }
}