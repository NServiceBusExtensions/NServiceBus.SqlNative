using Newtonsoft.Json;
using NServiceBus.Transport.SqlServerNative;
using Verify;
using Verify.SqlServer;
using Xunit;

[GlobalSetUp]
public static class GlobalSetup
{
    public static void Setup()
    {
        VerifySqlServer.Enable();
        SharedVerifySettings.ModifySerialization(settings =>
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