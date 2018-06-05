using System;
using System.Data.SqlClient;
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
        internal Func<CancellationToken, Task<SqlConnection>> ConnectionFunc;
        internal string OriginatingMachine = Environment.MachineName;
        internal string OriginatingEndpoint = "SqlHttpPassthrough";
        internal Func<HttpContext, PassthroughMessage, Task<Table>> SendCallback;
        internal Table DeduplicationTable = "Deduplication";
        internal Table AttachmentsTable = "MessageAttachments";
        internal string ClaimsHeaderPrefix;
        internal bool AppendClaims;

        /// <summary>
        /// Initialize a new instance of <see cref="PassthroughConfiguration"/>.
        /// </summary>
        /// <param name="connectionFunc">Creates a instance of a new and open <see cref="SqlConnection"/>.</param>
        /// <param name="callback">Manipulate or verify a <see cref="PassthroughMessage"/> prior to it being sent. Returns the destination <see cref="Table"/>.</param>
        public PassthroughConfiguration(
            Func<CancellationToken, Task<SqlConnection>> connectionFunc,
            Func<HttpContext, PassthroughMessage, Task<Table>> callback)
        {
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
            DeduplicationTable = table;
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