using Newtonsoft.Json;
using NServiceBus.Transport.SqlServerNative;
using VerifyXunit;
using Xunit;

[GlobalSetUp]
public static class GlobalSetup
{
    public static void Setup()
    {
        Global.ModifySerialization(settings =>
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