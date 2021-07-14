using System;
using System.Data;
using System.Data.Common;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        protected override DbCommand BuildReadCommand(int batchSize, long startRowVersion)
        {
            var command = Connection.CreateCommand(Transaction, string.Format(ReadSql, Table, batchSize));
            var parameter = command.CreateParameter();
            parameter.ParameterName = "RowVersion";
            parameter.DbType = DbType.Int64;
            parameter.Value = startRowVersion;
            command.Parameters.Add(parameter);
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

#if NETSTANDARD2_1
        protected override IncomingDelayedMessage ReadMessage(DbDataReader dataReader, params IAsyncDisposable[] cleanups)
#else
        protected override IncomingDelayedMessage ReadMessage(DbDataReader dataReader, params IDisposable[] cleanups)
#endif
        {
            var rowVersion = dataReader.GetInt64(0);
            var due = dataReader.DatetimeOrNull(1);
            var headers = dataReader.GetFieldValue<string>(2);
            var length = dataReader.LongOrNull(3);
            StreamWrapper? streamWrapper;
            if (length == null)
            {
                streamWrapper = null;
            }
            else
            {
                streamWrapper = new(length.Value, dataReader.GetStream(4));
            }

            return new(
                rowVersion: rowVersion,
                due: due,
                headers: headers,
                body: streamWrapper,
                cleanups
            );
        }
    }
}