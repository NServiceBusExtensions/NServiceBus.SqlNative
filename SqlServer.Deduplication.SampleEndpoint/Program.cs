using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;
using SampleNamespace;

class Program
{
    static async Task Main()
    {
        var defaultFactory = LogManager.Use<DefaultFactory>();
        defaultFactory.Level(LogLevel.Info);

        var connection = @"Server=.\SQLExpress;Database=DeduplicationSample; Integrated Security=True;Max Pool Size=100";
        var configuration = new EndpointConfiguration("SampleEndpoint");
        configuration.EnableInstallers();
        configuration.UsePersistence<LearningPersistence>();
        configuration.EnableDedup(async token =>
        {
            var sqlConnection = new SqlConnection(connection);
            await sqlConnection.OpenAsync(token);
            return sqlConnection;
        });
        configuration.UseSerialization<NewtonsoftSerializer>();
        configuration.DisableFeature<MessageDrivenSubscriptions>();
        configuration.DisableFeature<TimeoutManager>();
        configuration.PurgeOnStartup(true);
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(connection);
        configuration.EnableInstallers();
        Console.Title = "SampleEndpoint Press Ctrl-C to Exit.";
        Console.TreatControlCAsInput = true;
        var endpoint = await Endpoint.Start(configuration).ConfigureAwait(false);
        await SendMessages(endpoint);
        Console.ReadKey(true);
        await endpoint.Stop();
    }

    static async Task SendMessages(IEndpointInstance endpoint)
    {
        var message = new SampleMessage();
        var sendOptions = new SendOptions();
        sendOptions.RouteToThisEndpoint();
        sendOptions.SetMessageId(Guid.NewGuid().ToString());
        await endpoint.Send(message, sendOptions);
        Console.WriteLine("send succeeded");
        await endpoint.Send(message, sendOptions);
    }
}