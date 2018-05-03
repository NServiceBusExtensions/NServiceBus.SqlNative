using System;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract class MessageLoop : IDisposable
    {
        Action<Exception> errorCallback;
        Task task;
        CancellationTokenSource tokenSource;
        TimeSpan delay;

        public MessageLoop(
            Action<Exception> errorCallback,
            TimeSpan? delay = null)
        {
            Guard.AgainstNull(errorCallback, nameof(errorCallback));
            Guard.AgainstNegativeAndZero(delay, nameof(delay));
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
                        await RunBatch(cancellation).ConfigureAwait(false);

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

        protected abstract Task RunBatch(CancellationToken cancellation);

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