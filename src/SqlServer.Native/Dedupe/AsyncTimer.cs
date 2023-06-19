class AsyncTimer
{
    public virtual void Start(Func<DateTime, Cancel, Task> callback, TimeSpan interval, Action<Exception> errorCallback, Func<TimeSpan, Cancel, Task> delayStrategy)
    {
        tokenSource = new();
        var cancel = tokenSource.Token;

        task = Task.Run(async () =>
            {
                while (!cancel.IsCancellationRequested)
                {
                    try
                    {
                        var utcNow = DateTime.UtcNow;
                        await delayStrategy(interval, cancel);
                        await callback(utcNow, cancel);
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
            cancel);
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
    CancelSource? tokenSource;
}