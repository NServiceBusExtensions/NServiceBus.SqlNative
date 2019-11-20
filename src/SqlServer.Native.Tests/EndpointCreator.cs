﻿using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Transport.SQLServer;
using NServiceBus.Transport.SqlServerNative;

static class EndpointCreator
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
        var delayedDelivery = transport.NativeDelayedDelivery();
        delayedDelivery.DisableTimeoutManagerCompatibility();
        configuration.PurgeOnStartup(true);
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseSerialization<NewtonsoftSerializer>();
        return configuration;
    }
}