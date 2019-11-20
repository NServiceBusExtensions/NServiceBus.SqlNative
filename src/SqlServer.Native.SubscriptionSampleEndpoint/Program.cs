using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Transport.SqlServerNative;
using SampleNamespace;

class Program
{
    const string connection = @"Server=.\SQLExpress;Database=SubscriptionSample; Integrated Security=True;Max Pool Size=100";
    static async Task Main()
    {
        await CreateTables();
        var defaultFactory = LogManager.Use<DefaultFactory>();
        defaultFactory.Level(LogLevel.Info);

        var configuration = new EndpointConfiguration("SampleEndpoint");
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseSerialization<NewtonsoftSerializer>();
        configuration.PurgeOnStartup(true);
        configuration.DisableFeature<TimeoutManager>();
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(connection);
        transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);

        configuration.EnableInstallers();
        Console.Title = "SampleEndpoint Press Ctrl-C to Exit.";
        Console.TreatControlCAsInput = true;
        var endpoint = await Endpoint.Start(configuration);
        await Publish(endpoint);
        Console.ReadKey(true);
        await endpoint.Stop();
    }

    static async Task CreateTables()
    {
        await using var dbConnection = await ConnectionBuilder();
        var main = new QueueManager("SampleEndpoint", dbConnection);
        await main.Create();
        var delayed = new DelayedQueueManager("SampleEndpoint.Delayed", dbConnection);
        await delayed.Create();
        var subscription = new SubscriptionManager("SubscriptionRouting", dbConnection);
        await subscription.Create();
    }

    static async Task<DbConnection> ConnectionBuilder()
    {
        var sqlConnection = new SqlConnection(connection);
        try
        {
            await sqlConnection.OpenAsync();
            return sqlConnection;
        }
        catch
        {
            await sqlConnection.DisposeAsync();
            throw;
        }
    }

    static Task Publish(IEndpointInstance endpoint)
    {
        var message = new SampleMessage();
        return endpoint.Publish(message);
    }
}