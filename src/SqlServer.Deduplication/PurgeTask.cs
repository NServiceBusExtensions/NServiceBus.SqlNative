using Microsoft.Data.SqlClient;
using NServiceBus.Features;
using NServiceBus.Transport.SqlServerDeduplication;

class PurgeTask :
    FeatureStartupTask
{
    Table table;
    Func<Cancellation, Task<SqlConnection>> connectionBuilder;

    public PurgeTask(Table table, Func<Cancellation, Task<SqlConnection>> connectionBuilder)
    {
        this.table = table;
        this.connectionBuilder = connectionBuilder;
    }

    protected override async Task OnStart(IMessageSession session, Cancellation cancellation = default)
    {
        using var connection = await connectionBuilder(Cancellation.None);
        var dedupeManager = new DedupeManager(connection, table);
        await dedupeManager.PurgeItems(cancellation);
    }

    protected override Task OnStop(IMessageSession session, Cancellation cancellation = default) =>
        Task.CompletedTask;
}