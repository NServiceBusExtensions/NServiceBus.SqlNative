using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Transport.SqlServerDeduplication;

class MyStartupTask:FeatureStartupTask
{
    Table table;
    CriticalError criticalError;
    Func<CancellationToken, Task<SqlConnection>> connectionBuilder;
    DeduplicationCleanerJob job;

    public MyStartupTask(Table table, CriticalError criticalError, Func<CancellationToken, Task<SqlConnection>> connectionBuilder)
    {
        this.table = table;
        this.criticalError = criticalError;
        this.connectionBuilder = connectionBuilder;
    }

    protected override Task OnStart(IMessageSession session)
    {
        job = new DeduplicationCleanerJob(table, connectionBuilder, x => criticalError.Raise("Dedup cleanup failed", x));
        job.Start();
        return Task.CompletedTask;
    }

    protected override Task OnStop(IMessageSession session)
    {
        return job.Stop();
    }
}