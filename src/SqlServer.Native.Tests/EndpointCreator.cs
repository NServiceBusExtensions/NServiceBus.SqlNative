﻿static class EndpointCreator
{
    public static async Task<EndpointConfiguration> Create(string endpointName)
    {
        await using (var connection = Connection.OpenConnection())
        {
            var manager = new QueueManager(endpointName, connection);
            await manager.Create();
        }

        var configuration = new EndpointConfiguration(endpointName);
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(Connection.ConnectionString);
        transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
        transport.NativeDelayedDelivery();
        configuration.PurgeOnStartup(true);
        configuration.EnableInstallers();
        configuration.EnableDedupe(Connection.OpenAsyncConnection);
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseSerialization<NewtonsoftJsonSerializer>();
        configuration.AssemblyScanner()
            .ExcludeAssemblies("xunit.runner.utility.netcoreapp10.dll");
        return configuration;
    }
}