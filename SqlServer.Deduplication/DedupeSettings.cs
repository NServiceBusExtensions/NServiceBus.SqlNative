using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using NServiceBus.Transport.SqlServerDeduplication;

namespace NServiceBus
{
    public class DedupeSettings
    {
        internal Table Table = "Deduplication";
        internal Func<CancellationToken, Task<SqlConnection>> ConnectionBuilder;

        internal DedupeSettings(Func<CancellationToken, Task<SqlConnection>> connectionBuilder)
        {
            ConnectionBuilder = connectionBuilder;
        }

        internal bool RunCleanTask = true;
        internal bool InstallerDisabled;

        /// <summary>
        /// Disable the attachment cleanup task.
        /// </summary>
        public void DisableCleanupTask()
        {
            RunCleanTask = false;
        }

        /// <summary>
        /// Called when a message is deduplicated.
        /// </summary>
        public void Callback(Action<IOutgoingPhysicalMessageContext> action)
        {
            Guard.AgainstNull(action, nameof(action));
            CallbackAction = action;
        }

        internal Action<IOutgoingPhysicalMessageContext> CallbackAction;

        /// <summary>
        /// Control the table and schema used for deduplication.
        /// Defaults to 'dbo.Deduplication'.
        /// </summary>
        public void UseTable(Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            Table = table;
        }

        /// <summary>
        /// Disable the table creation installer.
        /// </summary>
        public void DisableInstaller()
        {
            InstallerDisabled = true;
        }
    }
}