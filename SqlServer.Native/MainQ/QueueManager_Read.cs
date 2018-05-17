using System;
using System.Data;
using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        protected override SqlCommand BuildReadCommand(int batchSize, long startRowVersion)
        {
            var command = connection.CreateCommand(transaction, string.Format(ReadSql, fullTableName, batchSize));
            command.Parameters.Add("RowVersion", SqlDbType.BigInt).Value = startRowVersion;
            return command;
        }

        public static readonly string ReadSql = ConnectionHelpers.WrapInNoCount(@"
select top({1})
    Id,
    RowVersion,
    Expires,
    Headers,
    datalength(Body),
    Body
from {0}
with (readpast)
where RowVersion >= @RowVersion
order by RowVersion
");

        protected override IncomingMessage ReadMessage(SqlDataReader dataReader, params IDisposable[] cleanups)
        {
            var id = dataReader.GetGuid(0);
            var rowVersion = dataReader.GetInt64(1);
            var expires = dataReader.ValueOrNull<DateTime?>(2);
            var headers = dataReader.ValueOrNull<string>(3);
            var length = dataReader.ValueOrNull<long?>(4);
            StreamWrapper streamWrapper;
            if (length == null)
            {
                streamWrapper = null;
            }
            else
            {
                streamWrapper = new StreamWrapper(length.Value, dataReader.GetStream(5));
            }

            return new IncomingMessage(
                id: id,
                rowVersion: rowVersion,
                expires: expires,
                headers: headers,
                body: streamWrapper,
                cleanups
            );
        }
    }
}