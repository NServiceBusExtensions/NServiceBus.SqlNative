using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NServiceBus.Transport.SqlServerNative;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    public class PassthroughConfiguration
    {
        internal Func<CancellationToken, Task<SqlConnection>> connectionFunc;
        internal string originatingMachine = Environment.MachineName;
        internal string originatingEndpoint = "SqlHttpPassThrough";
        internal Func<HttpContext, PassthroughMessage, Task<Table>> sendCallback = (context, message) =>
        {
            Table table = message.Destination;
            return Task.FromResult(table);
        };
        internal Table deduplicationTable = "Deduplication";
        internal Table attachmentsTable = new Table("MessageAttachments");

        public PassthroughConfiguration(
            Func<CancellationToken, Task<SqlConnection>> connectionFunc)
        {
            Guard.AgainstNull(connectionFunc, nameof(connectionFunc));
            this.connectionFunc = connectionFunc.WrapFunc(nameof(connectionFunc));
        }

        public void OriginatingInfo(string endpoint, string machine)
        {
            Guard.AgainstNullOrEmpty(endpoint, nameof(endpoint));
            Guard.AgainstNullOrEmpty(machine, nameof(machine));
            originatingMachine = machine;
            originatingEndpoint = endpoint;
        }

        public void SendingCallback(Func<HttpContext, PassthroughMessage, Task<Table>> callback)
        {
            Guard.AgainstNull(callback, nameof(callback));
            sendCallback = callback.WrapFunc(nameof(SendingCallback));
        }

        public void Deduplication(Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            deduplicationTable = table;
        }

        public void Attachments(Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            attachmentsTable = table;
        }
    }
}