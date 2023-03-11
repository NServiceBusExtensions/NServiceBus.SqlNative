using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using NServiceBus.Transport.SqlServerNative;

class DedupService :
    IHostedService
{
    Table table;
    Func<Cancellation, Task<SqlConnection>> connectionBuilder;
    Action<Exception> criticalError;
    DedupeCleanerJob? job;

    public DedupService(Table table, Func<Cancellation, Task<SqlConnection>> connectionBuilder, Action<Exception> criticalError)
    {
        this.table = table;
        this.connectionBuilder = connectionBuilder;
        this.criticalError = criticalError;
    }

    public Task StartAsync(Cancellation cancellation)
    {
        job = new(table, connectionBuilder, criticalError);
        job.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(Cancellation cancellation)
    {
        if (job != null)
        {
            return job.Stop();
        }

        return Task.CompletedTask;
    }
}