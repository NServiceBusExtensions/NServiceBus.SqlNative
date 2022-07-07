using NServiceBus;
using NServiceBus.Attachments.Sql;

class Program
{
    static async Task Main()
    {
        var configuration = new EndpointConfiguration("SampleEndpoint");
        configuration.UsePersistence<LearningPersistence>();
        var attachments = configuration.EnableAttachments(async () => await Connection.OpenAsyncConnection(), TimeToKeep.Default);
        attachments.UseTransportConnectivity();
        configuration.UseSerialization<NewtonsoftJsonSerializer>();
        configuration.PurgeOnStartup(true);
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(Connection.ConnectionString);
        configuration.EnableInstallers();
        Console.Title = "SampleEndpoint Press Ctrl-C to Exit.";
        Console.TreatControlCAsInput = true;
        var endpoint = await Endpoint.Start(configuration);
        Console.ReadKey(true);
        await endpoint.Stop();
    }
}