using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using NServiceBus.Transport.SqlServerNative;

class DedupService(Table table, Func<Cancel, Task<SqlConnection>> builder, Action<Exception> error) :
        IHostedService
{
    DedupeCleanerJob? job;

    public Task StartAsync(Cancel cancel)
    {
        job = new(table, builder, error);
        job.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(Cancel cancel)
    {
        if (job != null)
        {
            return job.Stop();
        }

        return Task.CompletedTask;
    }
}