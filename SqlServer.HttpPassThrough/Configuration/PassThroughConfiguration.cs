using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NServiceBus.SqlServer.HttpPassThrough
{
    public class PassThroughConfiguration
    {
        internal Func<CancellationToken, Task<SqlConnection>> connectionFunc;
        internal string originatingMachine = Environment.MachineName;
        internal string originatingEndpoint = "SqlHttpPassThrough";
        internal Action<HttpContext, PassThroughMessage> sendCallback = (context, message) => { };
        internal string deduplicationSchema;
        internal string deduplicationTable;
        internal bool deduplicationSanitize;

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

        public void Deduplication(string table, string schema, bool sanitize = true)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNullOrEmpty(schema, nameof(schema));
            deduplicationSchema = schema;
            deduplicationTable = table;
            deduplicationSanitize = sanitize;
        }
    }
}