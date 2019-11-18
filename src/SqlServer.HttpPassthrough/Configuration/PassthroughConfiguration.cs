using System;
using System.Data;
using System.Data.Common;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus.Transport.SqlServerNative;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    /// <summary>
    /// Configuration stat to be passed to <see cref="ConfigurationExtensions.AddSqlHttpPassthrough(IServiceCollection,PassthroughConfiguration)"/>.
    /// </summary>
    public class PassthroughConfiguration
    {
        internal Action<Exception> DedupCriticalError;
        internal Func<CancellationToken, Task<DbConnection>> ConnectionFunc;
        internal string OriginatingMachine = Environment.MachineName;
        internal string OriginatingEndpoint = "SqlHttpPassthrough";
        internal Func<HttpContext, PassthroughMessage, Task<Table>> SendCallback;
        internal Table DedupeTable = "Deduplication";
        internal Table AttachmentsTable = "MessageAttachments";
        internal string? ClaimsHeaderPrefix;
        internal bool AppendClaims;

        /// <summary>
        /// Initialize a new instance of <see cref="PassthroughConfiguration"/>.
        /// </summary>
        /// <param name="connectionFunc">Creates a instance of a new and un-open <see cref="DbConnection"/>.</param>
        /// <param name="callback">Manipulate or verify a <see cref="PassthroughMessage"/> prior to it being sent. Returns the destination <see cref="Table"/>.</param>
        /// <param name="dedupCriticalError">Called when failed to clean expired records after 10 consecutive unsuccessful attempts. The most likely cause of this is connectivity issues with the database.</param>
        public PassthroughConfiguration(
            Func<DbConnection> connectionFunc,
            Func<HttpContext, PassthroughMessage, Task<Table>> callback,
            Action<Exception> dedupCriticalError) :
            this(
                connectionFunc: async token =>
                {
                    var connection = connectionFunc();
                    if (connection.State == ConnectionState.Open)
                    {
                        throw new Exception("This overload of PassthroughConfiguration expects `Func<DbConnection> connectionFunc` to return a un-opened DbConnection.");
                    }
                    try
                    {
                        await connection.OpenAsync(token).ConfigureAwait(false);
                        return connection;
                    }
                    catch
                    {
                        connection.Dispose();
                        throw;
                    }
                },
                callback,
                dedupCriticalError)
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="PassthroughConfiguration"/>.
        /// </summary>
        /// <param name="connectionFunc">Creates a instance of a new and open <see cref="DbConnection"/>.</param>
        /// <param name="callback">Manipulate or verify a <see cref="PassthroughMessage"/> prior to it being sent. Returns the destination <see cref="Table"/>.</param>
        /// <param name="dedupCriticalError">Called when failed to clean expired records after 10 consecutive unsuccessful attempts. The most likely cause of this is connectivity issues with the database.</param>
        public PassthroughConfiguration(
            Func<CancellationToken, Task<DbConnection>> connectionFunc,
            Func<HttpContext, PassthroughMessage, Task<Table>> callback,
            Action<Exception> dedupCriticalError)
        {
            DedupCriticalError = dedupCriticalError;
            Guard.AgainstNull(connectionFunc, nameof(connectionFunc));
            Guard.AgainstNull(callback, nameof(callback));
            SendCallback = callback.WrapFunc(nameof(callback));
            ConnectionFunc = connectionFunc.WrapFunc(nameof(connectionFunc));
        }

        /// <summary>
        /// Control the values used for 'NServiceBus.OriginatingEndpoint' and 'NServiceBus.OriginatingMachine'.
        /// </summary>
        public void OriginatingInfo(string endpoint, string machine)
        {
            Guard.AgainstNullOrEmpty(endpoint, nameof(endpoint));
            Guard.AgainstNullOrEmpty(machine, nameof(machine));
            OriginatingMachine = machine;
            OriginatingEndpoint = endpoint;
        }

        /// <summary>
        /// Control the table and schema used for deduplication.
        /// Defaults to 'dbo.Deduplication'.
        /// </summary>
        public void Deduplication(Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            DedupeTable = table;
        }

        /// <summary>
        /// Control the table and schema used for attachments.
        /// Defaults to 'dbo.MessageAttachments'.
        /// </summary>
        public void Attachments(Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            AttachmentsTable = table;
        }

        /// <summary>
        /// Append the <see cref="Claim"/>s of the <see cref="ClaimsPrincipal"/> from <see cref="HttpContext.User"/>.
        /// </summary>
        /// <param name="headerPrefix">The key prefix to use on the outgoing message header. Defaults to 'SqlHttpPassthrough.Claim.'.</param>
        public void AppendClaimsToMessageHeaders(string headerPrefix = "SqlHttpPassthrough.Claim.")
        {
            Guard.AgainstNullOrEmpty(headerPrefix, nameof(headerPrefix));
            AppendClaims = true;
            ClaimsHeaderPrefix = headerPrefix;
        }
    }
}