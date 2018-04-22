using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    public class Receiver
    {
        public virtual async Task<Message> Receive(string connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation);
                return await InnerReceive(sqlConnection, null, table, cancellation);
            }
        }

        public virtual Task<Message> Receive(SqlConnection connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerReceive(connection, null, table, cancellation);
        }

        public virtual Task<Message> Receive(SqlConnection connection, SqlTransaction transaction, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerReceive(connection, transaction, table, cancellation);
        }

        static async Task<Message> InnerReceive(SqlConnection connection, SqlTransaction transaction, string table, CancellationToken cancellation)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = string.Format(ReceiveSql, table);

                var dataReader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow | CommandBehavior.SequentialAccess, cancellation).ConfigureAwait(false);
                if (!dataReader.Read())
                {
                    return null;
                }

                return new Message(
                    id: await dataReader.GetFieldValueAsync<Guid>(0, cancellation).ConfigureAwait(false),
                    correlationId: await dataReader.ValueOrNull<string>(1, cancellation).ConfigureAwait(false),
                    replyToAddress: await dataReader.ValueOrNull<string>(2, cancellation).ConfigureAwait(false),
                    expires: await dataReader.ValueOrNull<DateTime>(3, cancellation).ConfigureAwait(false),
                    headers: await dataReader.ValueOrNull<string>(4, cancellation).ConfigureAwait(false),
                    body: await dataReader.ValueOrNull<byte[]>(5, cancellation).ConfigureAwait(false)
                );
            }
        }

        public static readonly string ReceiveSql = @"
DECLARE @NOCOUNT VARCHAR(3) = 'OFF';
IF ( (512 & @@OPTIONS) = 512 ) SET @NOCOUNT = 'ON';
SET NOCOUNT ON;

WITH message AS (
    SELECT TOP(1) *
    FROM {0} WITH (UPDLOCK, READPAST, ROWLOCK)
    ORDER BY RowVersion)
DELETE FROM message
OUTPUT
    deleted.Id,
    deleted.CorrelationId,
    deleted.ReplyToAddress,
    deleted.Expires,
    deleted.Headers,
    deleted.Body;

IF (@NOCOUNT = 'ON') SET NOCOUNT ON;
IF (@NOCOUNT = 'OFF') SET NOCOUNT OFF;";
    }
}