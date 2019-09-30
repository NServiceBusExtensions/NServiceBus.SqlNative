using Newtonsoft.Json;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;

public static class ModuleInitializer
{
    public static void Initialize()
    {
        SerializerBuilder.ExtraSettings = settings =>
        {
            settings.TypeNameHandling = TypeNameHandling.Objects;
        };
        SqlHelper.EnsureDatabaseExists(Connection.ConnectionString);
        using var sqlConnection = Connection.OpenConnection();
        var manager = new QueueManager("error", sqlConnection);
        manager.Create().Await();
    }
}