class AsyncTimer
{
    public virtual void Start(Func<DateTime, Cancellation, Task> callback, TimeSpan interval, Action<Exception> errorCallback, Func<TimeSpan, Cancellation, Task> delayStrategy)
    {
        tokenSource = new();
        var cancellation = tokenSource.Token;

        task = Task.Run(async () =>
            {
                while (!cancellation.IsCancellationRequested)
                {
                    try
                    {
                        var utcNow = DateTime.UtcNow;
                        await delayStrategy(interval, cancellation);
                        await callback(utcNow, cancellation);
                    }
                    catch (OperationCanceledException)
                    {
                        // noop
                    }
                    catch (Exception ex)
                    {
                        errorCallback(ex);
                    }
                }
            },
            cancellation);
    }

    public virtual Task Stop()
    {
        if (tokenSource == null)
        {
            return Task.FromResult(0);
        }

        tokenSource.Cancel();
        tokenSource.Dispose();

        return task ?? Task.FromResult(0);
    }

    Task? task;
    CancellationSource? tokenSource;
}