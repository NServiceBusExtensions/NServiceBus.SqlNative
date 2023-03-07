﻿namespace NServiceBus.Transport.SqlServerNative;

public abstract class MessageLoop :
    IAsyncDisposable
{
    Action<Exception> errorCallback;
    Task? task;
    CancellationTokenSource? tokenSource;
    TimeSpan delay;

    public MessageLoop(
        Action<Exception> errorCallback,
        TimeSpan? delay = null)
    {
        Guard.AgainstNegativeAndZero(delay, nameof(delay));
        this.errorCallback = errorCallback.WrapFunc(nameof(errorCallback));
        this.delay = delay.GetValueOrDefault(TimeSpan.FromMinutes(1));
    }

    public void Start()
    {
        tokenSource = new();
        var cancellation = tokenSource.Token;

        task = Task.Run(async () =>
            {
                while (!cancellation.IsCancellationRequested)
                {
                    try
                    {
                        await RunBatch(cancellation);

                        await Task.Delay(delay, cancellation);
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

    protected abstract Task RunBatch(Cancellation cancellation);

    public Task Stop()
    {
        tokenSource?.Cancel();
        tokenSource?.Dispose();
        if (task == null)
        {
            return Task.CompletedTask;
        }

        return task;
    }

    public ValueTask DisposeAsync() => new(Stop());
}