using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    public partial class Receiver
    {
        string table;

        public Receiver(string table)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            this.table = table;
        }

        SqlCommand BuildCommand(SqlConnection connection, SqlTransaction transaction, int batchSize)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = string.Format(ReceiveSql, table, batchSize);
            return command;
        }

        static async Task<Message> ReadMessage(CancellationToken cancellation, SqlDataReader dataReader)
        {
            return new Message(
                id: await dataReader.GetFieldValueAsync<Guid>(0, cancellation).ConfigureAwait(false),
                correlationId: await dataReader.ValueOrNull<string>(1, cancellation).ConfigureAwait(false),
                replyToAddress: await dataReader.ValueOrNull<string>(2, cancellation).ConfigureAwait(false),
                expires: await dataReader.ValueOrNull<DateTime?>(3, cancellation).ConfigureAwait(false),
                headers: await dataReader.ValueOrNull<string>(4, cancellation).ConfigureAwait(false),
                body: await dataReader.ValueOrNull<byte[]>(5, cancellation).ConfigureAwait(false)
            );
        }

        public static readonly string ReceiveSql = @"
declare @nocount varchar(3) = 'off';
if ( (512 & @@options) = 512 ) set @nocount = 'on';
set nocount on;

with message as (
    select top({1}) *
    from {0} with (updlock, readpast, rowlock)
    order by RowVersion)
delete from message
output
    deleted.Id,
    deleted.CorrelationId,
    deleted.ReplyToAddress,
    deleted.Expires,
    deleted.Headers,
    deleted.Body;

if (@nocount = 'on') set nocount on;
if (@nocount = 'off') set nocount off;";
    }
}