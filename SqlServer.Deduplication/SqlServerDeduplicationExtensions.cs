using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Configuration.AdvancedExtensibility;
using NServiceBus.Extensibility;

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
            var recoverability = configuration.Recoverability();
            recoverability.AddUnrecoverableException<NotSupportedException>();
            var settings = configuration.GetSettings();
            var deduplicationSettings = new DeduplicationSettings(connectionBuilder);
            settings.Set(deduplicationSettings);
            configuration.EnableFeature<DeduplicationFeature>();
            return deduplicationSettings;
        }

        public static Task SendWithDeduplication(this IMessageSession session, Guid messageId, object message, SendOptions options = null)
        {
            Guard.AgainstEmpty(messageId, nameof(messageId));
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNull(session, nameof(session));
            if (options == null)
            {
                options = new SendOptions();
            }
            else
            {
                ValidateMessageId(options);
            }

            return InnerSendWithDeduplication(session, message, messageId, options);
        }

        static void ValidateMessageId(SendOptions options)
        {
            if (options.GetMessageId() != null)
            {
                throw new ArgumentException("Expected a SendOptions with no MessageId defined", nameof(options));
            }
        }

        static Task InnerSendWithDeduplication(IMessageSession session, object message, Guid messageId, SendOptions options)
        {
            options.GetExtensions().Set("SqlServer.Deduplication", true);
            options.SetMessageId(messageId.ToString());
            return session.Send(message, options);
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