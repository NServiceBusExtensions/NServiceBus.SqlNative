using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NServiceBus.Transport.SqlServerNative;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    public class PassThroughConfiguration
    {
        internal Func<CancellationToken, Task<SqlConnection>> connectionFunc;
        internal string originatingMachine = Environment.MachineName;
        internal string originatingEndpoint = "SqlHttpPassThrough";
        internal Action<HttpContext, PassThroughMessage> sendCallback = (context, message) => { };
        internal Table deduplicationTable = "Deduplication";
        internal Table attachmentsTable = new Table("MessageAttachments");

        public PassThroughConfiguration(
            Func<CancellationToken, Task<SqlConnection>> connectionFunc)
        {
            Guard.AgainstNull(connectionFunc, nameof(connectionFunc));
            this.connectionFunc = connectionFunc;
        }

        public void OriginatingInfo(string endpoint, string machine)
        {
            Guard.AgainstNullOrEmpty(endpoint, nameof(endpoint));
            Guard.AgainstNullOrEmpty(machine, nameof(machine));
            originatingMachine = machine;
            originatingEndpoint = endpoint;
        }

        public void SendingCallback(Action<HttpContext, PassThroughMessage> callback)
        {
            Guard.AgainstNull(callback, nameof(callback));
            sendCallback = callback;
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