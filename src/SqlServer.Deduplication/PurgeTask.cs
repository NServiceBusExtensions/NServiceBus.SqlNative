using Microsoft.Data.SqlClient;
using NServiceBus.Features;
using NServiceBus.Transport.SqlServerDeduplication;

class PurgeTask :
    FeatureStartupTask
{
    Table table;
    Func<Cancel, Task<SqlConnection>> connectionBuilder;

    public PurgeTask(Table table, Func<Cancel, Task<SqlConnection>> connectionBuilder)
    {
        this.table = table;
        this.connectionBuilder = connectionBuilder;
    }

    protected override async Task OnStart(IMessageSession session, Cancel cancel = default)
    {
        using var connection = await connectionBuilder(Cancel.None);
        var dedupeManager = new DedupeManager(connection, table);
        await dedupeManager.PurgeItems(cancel);
    }

    protected override Task OnStop(IMessageSession session, Cancel cancel = default) =>
        Task.CompletedTask;
}