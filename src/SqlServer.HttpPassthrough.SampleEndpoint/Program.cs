using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Attachments.Sql;
using NServiceBus.Features;

class Program
{
    static async Task Main()
    {
        var configuration = new EndpointConfiguration("SampleEndpoint");
        configuration.UsePersistence<LearningPersistence>();
        var attachments = configuration.EnableAttachments(async () => await Connection.OpenAsyncConnection(), TimeToKeep.Default);
        attachments.UseTransportConnectivity();
        configuration.UseSerialization<NewtonsoftSerializer>();
        configuration.DisableFeature<MessageDrivenSubscriptions>();
        configuration.DisableFeature<TimeoutManager>();
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