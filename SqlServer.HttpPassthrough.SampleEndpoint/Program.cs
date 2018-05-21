using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Attachments.Sql;
using NServiceBus.Features;

class Program
{
    static async Task Main()
    {
        var connection = @"Server=.\SQLExpress;Database=MessageHttpPassThroughTests; Integrated Security=True;Max Pool Size=100";
        var configuration = new EndpointConfiguration("SampleEndpoint");
        configuration.UsePersistence<LearningPersistence>();
        configuration.EnableAttachments(connection, TimeToKeep.Default);
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
        Console.ReadKey(true);
        await endpoint.Stop();
    }
}