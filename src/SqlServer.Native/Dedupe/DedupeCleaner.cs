using System;
using System.Threading;
using System.Threading.Tasks;

class DedupeCleaner
{
    public virtual Task Stop() => timer.Stop();

    AsyncTimer timer;
    Action<Exception> criticalError;
    Func<CancellationToken, Task> cleanup;
    TimeSpan frequencyToRunCleanup;

    public DedupeCleaner(Func<CancellationToken, Task> cleanup, Action<Exception> criticalError, TimeSpan frequencyToRunCleanup, AsyncTimer timer)
    {
        this.cleanup = cleanup;
        this.frequencyToRunCleanup = frequencyToRunCleanup;
        this.timer = timer;
        this.criticalError = criticalError;
    }

    public virtual void Start()
    {
        var cleanupFailures = 0;
        timer.Start(
            callback: async (utcTime, token) =>
            {
                await cleanup(token);
                cleanupFailures = 0;
            },
            interval: frequencyToRunCleanup,
            errorCallback: exception =>
            {
                //TODO: log every exception
                cleanupFailures++;
                if (cleanupFailures >= 10)
                {
                    criticalError(exception);
                    cleanupFailures = 0;
                }
            },
            delayStrategy: Task.Delay);
    }

}