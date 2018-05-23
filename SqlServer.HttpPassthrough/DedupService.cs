using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NServiceBus.Transport.SqlServerNative;

class DedupService : IHostedService
{
    Table table;
    Func<CancellationToken, Task<SqlConnection>> connectionBuilder;
    DeduplicationCleanerJob job;

    public DedupService(Table table, Func<CancellationToken, Task<SqlConnection>> connectionBuilder)
    {
        this.table = table;
        this.connectionBuilder = connectionBuilder;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        void CriticalError(string message, Exception exception)
        {
            Environment.FailFast(message, exception);
        }

        job = new DeduplicationCleanerJob(table, connectionBuilder, CriticalError);
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