using Microsoft.Data.SqlClient;
using NServiceBus.Features;
using NServiceBus.Transport.SqlServerDeduplication;

class CleanupTask :
    FeatureStartupTask
{
    Table table;
    CriticalError criticalError;
    Func<Cancel, Task<SqlConnection>> connectionBuilder;
    DedupeCleanerJob? job;

    public CleanupTask(Table table, CriticalError criticalError,
        Func<Cancel, Task<SqlConnection>> connectionBuilder)
    {
        this.table = table;
        this.criticalError = criticalError;
        this.connectionBuilder = connectionBuilder;
    }

    protected override Task OnStart(IMessageSession session, Cancel cancel = default)
    {
        job = new(table, connectionBuilder, RaiseError);
        job.Start();
        return Task.CompletedTask;
    }

    void RaiseError(Exception exception) =>
        criticalError.Raise("Dedup cleanup failed", exception);

    protected override Task OnStop(IMessageSession session, Cancel cancel = default) =>
        job == null ? Task.CompletedTask : job.Stop();
}