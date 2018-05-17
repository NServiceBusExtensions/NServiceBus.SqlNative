using System;
using System.Data;
using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        protected override SqlCommand BuildReadCommand(int batchSize, long startRowVersion)
        {
            var command = connection.CreateCommand(transaction, string.Format(ReadSql, fullTableName, batchSize));
            command.Parameters.Add("RowVersion", SqlDbType.BigInt).Value = startRowVersion;
            return command;
        }

        public static readonly string ReadSql = ConnectionHelpers.WrapInNoCount(@"
select top({1})
    RowVersion,
    Due,
    Headers,
    datalength(Body),
    Body
from {0}
with (readpast)
where RowVersion >= @RowVersion
order by RowVersion
");

        protected override IncomingDelayedMessage ReadMessage(SqlDataReader dataReader, params IDisposable[] cleanups)
        {
            var rowVersion = dataReader.GetInt64(0);
            var due = dataReader.ValueOrNull<DateTime>(1);
            var headers = dataReader.ValueOrNull<string>(2);
            var length = dataReader.ValueOrNull<long?>(3);
            StreamWrapper streamWrapper;
            if (length == null)
            {
                streamWrapper = null;
            }
            else
            {
                streamWrapper = new StreamWrapper(length.Value, dataReader.GetStream(4));
            }

            return new IncomingDelayedMessage(
                rowVersion: rowVersion,
                due: due,
                headers: headers,
                body: streamWrapper,
                cleanups
            );
        }
    }
}