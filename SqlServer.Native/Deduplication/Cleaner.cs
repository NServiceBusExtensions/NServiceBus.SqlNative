using System;
using System.Threading;
using System.Threading.Tasks;

class Cleaner
{
    public Cleaner(Func<CancellationToken, Task> cleanup, Action<string, Exception> criticalError, TimeSpan frequencyToRunCleanup, AsyncTimer timer)
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
                await cleanup(token).ConfigureAwait(false);
                cleanupFailures = 0;
            },
            interval: frequencyToRunCleanup,
            errorCallback: exception =>
            {
                //TODO: log every exception
                cleanupFailures++;
                if (cleanupFailures >= 10)
                {
                    criticalError("Failed to clean expired records after 10 consecutive unsuccessful attempts. The most likely cause of this is connectivity issues with the database.", exception);
                    cleanupFailures = 0;
                }
            },
            delayStrategy: Task.Delay);
    }

    public virtual Task Stop() => timer.Stop();

    AsyncTimer timer;
    Action<string, Exception> criticalError;
    Func<CancellationToken, Task> cleanup;
    TimeSpan frequencyToRunCleanup;
}