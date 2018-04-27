using System;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract class MessageLoop : IDisposable
    {
        Func<IncomingBytesMessage, CancellationToken, Task> callback;
        Action<Exception> errorCallback;
        Task task;
        CancellationTokenSource tokenSource;
        TimeSpan delay;

        public MessageLoop(
            Func<IncomingBytesMessage, CancellationToken, Task> callback,
            Action<Exception> errorCallback,
            TimeSpan? delay = null)
        {
            Guard.AgainstNull(callback, nameof(callback));
            Guard.AgainstNull(errorCallback, nameof(errorCallback));
            Guard.AgainstNegativeAndZero(delay, nameof(delay));
            this.callback = callback;
            this.errorCallback = errorCallback;
            this.delay = delay.GetValueOrDefault(TimeSpan.FromMinutes(1));
        }

        public void Start()
        {
            tokenSource = new CancellationTokenSource();
            var cancellation = tokenSource.Token;

            task = Task.Run(async () =>
            {
                while (!cancellation.IsCancellationRequested)
                {
                    try
                    {
                        await RunBatch(callback,cancellation).ConfigureAwait(false);

                        await Task.Delay(delay, cancellation).ConfigureAwait(false);
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
            }, CancellationToken.None);
        }

        protected abstract Task RunBatch(Func<IncomingBytesMessage, CancellationToken, Task> callback, CancellationToken cancellation);

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

        public void Dispose()
        {
            Stop().GetAwaiter().GetResult();
        }
    }
}