using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace NServiceBus
{
    /// <summary>
    /// Extensions to control what messages are audited.
    /// </summary>
    public static class SqlServerDeduplicationExtensions
    {
        /// <summary>
        /// Enable SQL attachments for this endpoint.
        /// </summary>
        public static DeduplicationSettings EnableDedup(
            this EndpointConfiguration configuration,
            string connection)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            return EnableDedup(configuration, cancellation => OpenConnection(connection, cancellation));
        }

        /// <summary>
        /// Enable SQL attachments for this endpoint.
        /// </summary>
        public static DeduplicationSettings EnableDedup(
            this EndpointConfiguration configuration,
            Func<CancellationToken, Task<SqlConnection>> connectionBuilder)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(connectionBuilder, nameof(connectionBuilder));
            var settings = configuration.GetSettings();
            var deduplicationSettings = new DeduplicationSettings(connectionBuilder);
            settings.Set(deduplicationSettings);
            configuration.EnableFeature<DeduplicationFeature>();
            return deduplicationSettings;
        }

        static async Task<SqlConnection> OpenConnection(string connectionString, CancellationToken cancellation)
        {
            var connection = new SqlConnection(connectionString);
            try
            {
                await connection.OpenAsync(cancellation).ConfigureAwait(false);
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }
    }
}