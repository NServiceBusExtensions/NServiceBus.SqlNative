using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Transport.SqlServerDeduplication;

class CleanupTask : FeatureStartupTask
{
    Table table;
    CriticalError criticalError;
    Func<CancellationToken, Task<SqlConnection>> connectionBuilder;
    DedupeCleanerJob job;

    public CleanupTask(Table table, CriticalError criticalError,
        Func<CancellationToken, Task<SqlConnection>> connectionBuilder)
    {
        this.table = table;
        this.criticalError = criticalError;
        this.connectionBuilder = connectionBuilder;
    }

    protected override Task OnStart(IMessageSession session)
    {
        job = new DedupeCleanerJob(table, connectionBuilder, RaiseError);
        job.Start();
        return Task.CompletedTask;
    }

    void RaiseError(Exception exception)
    {
        criticalError.Raise("Dedup cleanup failed", exception);
    }

    protected override Task OnStop(IMessageSession session)
    {
        return job.Stop();
    }
}