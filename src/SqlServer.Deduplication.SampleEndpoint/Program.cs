using Microsoft.Data.SqlClient;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Transport.SqlServerDeduplication;
using SampleNamespace;

class Program
{
    const string connection = @"Server=.\SQLExpress;Database=DedupeSample; Integrated Security=True;Max Pool Size=100";
    static async Task Main()
    {
        var defaultFactory = LogManager.Use<DefaultFactory>();
        defaultFactory.Level(LogLevel.Info);

        var configuration = new EndpointConfiguration("SampleEndpoint");
        configuration.EnableInstallers();
        configuration.UsePersistence<LearningPersistence>();
        configuration.EnableDedupe(ConnectionBuilder);
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
        await SendMessages(endpoint);
        Console.ReadKey(true);
        await endpoint.Stop();
    }

    static async Task<SqlConnection> ConnectionBuilder(CancellationToken cancellation)
    {
        var sqlConnection = new SqlConnection(connection);
        try
        {
            await sqlConnection.OpenAsync(cancellation);
            return sqlConnection;
        }
        catch
        {
            await sqlConnection.DisposeAsync();
            throw;
        }
    }

    static async Task SendMessages(IEndpointInstance endpoint)
    {
        var guid = Guid.NewGuid();
        var dedupeOutcome1 = await SendMessage(endpoint, guid);
        Console.WriteLine($"DedupeOutcome:{dedupeOutcome1.DedupeOutcome}. Context:{dedupeOutcome1.Context}");
        var dedupeOutcome2 = await SendMessage(endpoint, guid);
        Console.WriteLine($"DedupeOutcome:{dedupeOutcome2.DedupeOutcome}. Context:{dedupeOutcome2.Context}");
    }

    static Task<DedupeResult> SendMessage(IEndpointInstance endpoint, Guid guid)
    {
        var message = new SampleMessage();
        var options = new SendOptions();
        options.RouteToThisEndpoint();
        return endpoint.SendWithDedupe(guid, message, options);
    }
}