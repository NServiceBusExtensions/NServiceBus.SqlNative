using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public class DeduplicationCleanerJob
    {
        Table table;
        Func<CancellationToken, Task<SqlConnection>> connectionBuilder;
        Action<string, Exception> criticalError;
        TimeSpan expireWindow;
        TimeSpan frequencyToRunCleanup;
        Cleaner cleaner;

        public DeduplicationCleanerJob(Table table, Func<CancellationToken, Task<SqlConnection>> connectionBuilder, Action<string, Exception> criticalError, TimeSpan? expireWindow = null,TimeSpan? frequencyToRunCleanup = null)
        {
            Guard.AgainstNull(table, nameof(table));
            Guard.AgainstNull(criticalError, nameof(criticalError));
            Guard.AgainstNull(connectionBuilder, nameof(connectionBuilder));
            Guard.AgainstNegativeAndZero(expireWindow, nameof(expireWindow));
            Guard.AgainstNegativeAndZero(frequencyToRunCleanup, nameof(frequencyToRunCleanup));
            this.expireWindow = expireWindow.GetValueOrDefault(TimeSpan.FromDays(10));
            this.frequencyToRunCleanup = frequencyToRunCleanup.GetValueOrDefault(TimeSpan.FromHours(1));
            this.table = table;
            this.connectionBuilder = connectionBuilder;
            this.criticalError = criticalError;
        }

        public virtual void Start()
        {
            cleaner = new Cleaner(async cancellation =>
                {
                    using (var connection = await connectionBuilder(cancellation).ConfigureAwait(false))
                    {
                        var deduplicationCleaner = new DeduplicationManager(connection, table);
                        var expiry = DateTime.UtcNow.Subtract(expireWindow);
                        await deduplicationCleaner.CleanupItemsOlderThan(expiry, cancellation)
                            .ConfigureAwait(false);
                    }
                },
                criticalError: criticalError,
                frequencyToRunCleanup: frequencyToRunCleanup,
                timer: new AsyncTimer());
            cleaner.Start();
        }

        public virtual Task Stop() => cleaner.Stop();
    }
}