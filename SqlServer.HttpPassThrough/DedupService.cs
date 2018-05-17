using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NServiceBus.Transport.SqlServerNative;

class DedupService : IHostedService
{
    Func<CancellationToken, Task<SqlConnection>> connectionBuilder;
    DeduplicationCleanerJob job;

    public DedupService(Func<CancellationToken, Task<SqlConnection>> connectionBuilder)
    {
        this.connectionBuilder = connectionBuilder;
    }

    //TODO: verify
    public Task StartAsync(CancellationToken cancellationToken)
    {
        void CriticalError(string message, Exception exception)
        {
            Environment.FailFast(message, exception);
        }

        job = new DeduplicationCleanerJob(connectionBuilder, CriticalError, table: "MessagePassThroughDeduplication");
        job.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return job.Stop();
    }
}