class DedupeCleaner(
    Func<Cancel, Task> cleanup,
    Action<Exception> error,
    TimeSpan toRunCleanup,
    AsyncTimer timer)
{
    public virtual Task Stop() => timer.Stop();

    public virtual void Start()
    {
        var cleanupFailures = 0;
        timer.Start(
            callback: async (_, token) =>
            {
                await cleanup(token);
                cleanupFailures = 0;
            },
            interval: toRunCleanup,
            errorCallback: exception =>
            {
                //TODO: log every exception
                cleanupFailures++;
                if (cleanupFailures >= 10)
                {
                    error(exception);
                    cleanupFailures = 0;
                }
            },
            delayStrategy: Task.Delay);
    }
}