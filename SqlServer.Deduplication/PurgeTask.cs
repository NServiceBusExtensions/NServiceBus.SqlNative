using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Transport.SqlServerDeduplication;

class PurgeTask : FeatureStartupTask
{
    Table table;
    Func<CancellationToken, Task<SqlConnection>> connectionBuilder;

    public PurgeTask(Table table, Func<CancellationToken, Task<SqlConnection>> connectionBuilder)
    {
        this.table = table;
        this.connectionBuilder = connectionBuilder;
    }

    protected override async Task OnStart(IMessageSession session)
    {
        using (var connection = await connectionBuilder(CancellationToken.None).ConfigureAwait(false))
        {
            var deduplicationCleaner = new DeduplicationManager(connection, table);
            await deduplicationCleaner.PurgeItems().ConfigureAwait(false);
        }
    }

    protected override Task OnStop(IMessageSession session)
    {
        return Task.CompletedTask;
    }
}