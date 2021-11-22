using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using NServiceBus.Transport.SqlServerNative;

class DedupService :
    IHostedService
{
    Table table;
    Func<CancellationToken, Task<SqlConnection>> connectionBuilder;
    Action<Exception> criticalError;
    DedupeCleanerJob? job;

    public DedupService(Table table, Func<CancellationToken, Task<SqlConnection>> connectionBuilder, Action<Exception> criticalError)
    {
        this.table = table;
        this.connectionBuilder = connectionBuilder;
        this.criticalError = criticalError;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        job = new(table, connectionBuilder, criticalError);
        job.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (job != null)
        {
            return job.Stop();
        }

        return Task.CompletedTask;
    }
}