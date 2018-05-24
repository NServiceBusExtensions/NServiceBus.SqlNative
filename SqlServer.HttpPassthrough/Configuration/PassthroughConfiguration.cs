using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus.Transport.SqlServerNative;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    /// <summary>
    /// Configuration stat to be passed to <see cref="ConfigurationExtensions.AddSqlHttpPassThrough(IServiceCollection,PassthroughConfiguration)"/>
    /// </summary>
    public class PassthroughConfiguration
    {
        internal Func<CancellationToken, Task<SqlConnection>> connectionFunc;
        internal string originatingMachine = Environment.MachineName;
        internal string originatingEndpoint = "SqlHttpPassThrough";
        internal Func<string, Table> convertDestination = destination => destination;
        internal Action<HttpContext, PassthroughMessage> sendCallback = (context, message) => { };
        internal Table deduplicationTable = "Deduplication";
        internal Table attachmentsTable = new Table("MessageAttachments");

        /// <summary>
        /// Initialise a new instance of <see cref="PassthroughConfiguration"/>
        /// </summary>
        /// <param name="connectionFunc">Creates a instance of a new and open <see cref="SqlConnection"/>.</param>
        public PassthroughConfiguration(
            Func<CancellationToken, Task<SqlConnection>> connectionFunc)
        {
            Guard.AgainstNull(connectionFunc, nameof(connectionFunc));
            this.connectionFunc = connectionFunc.WrapFunc(nameof(connectionFunc));
        }

        /// <summary>
        /// Control the values used for 'NServiceBus.OriginatingEndpoint' and 'NServiceBus.OriginatingMachine'.
        /// </summary>
        public void OriginatingInfo(string endpoint, string machine)
        {
            Guard.AgainstNullOrEmpty(endpoint, nameof(endpoint));
            Guard.AgainstNullOrEmpty(machine, nameof(machine));
            originatingMachine = machine;
            originatingEndpoint = endpoint;
        }

        /// <summary>
        /// Manipulate or verify a <see cref="PassthroughMessage"/> prior to it being sent.
        /// </summary>
        public void SendingCallback(Action<HttpContext, PassthroughMessage> callback)
        {
            Guard.AgainstNull(callback, nameof(callback));
            sendCallback = callback.WrapFunc(nameof(SendingCallback));
        }

        /// <summary>
        /// Converts an incoming <see cref="string"/> from a client to a <see cref="Table"/> (i.e. a sql schema and table).
        /// </summary>
        public void DestinationConverter(Func<string, Table> convert)
        {
            Guard.AgainstNull(convert, nameof(convert));
            convertDestination = convert.WrapFunc(nameof(DestinationConverter));
        }

        /// <summary>
        /// Control the table and schema used for deduplication.
        /// Defaults to 'dbo.Deduplication'.
        /// </summary>
        public void Deduplication(Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            deduplicationTable = table;
        }

        /// <summary>
        /// Control the table and schema used for attachments.
        /// Defaults to 'dbo.MessageAttachments'.
        /// </summary>
        public void Attachments(Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            attachmentsTable = table;
        }
    }
}