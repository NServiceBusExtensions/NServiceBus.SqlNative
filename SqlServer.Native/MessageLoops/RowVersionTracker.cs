using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public class RowVersionTracker
    {
        string table;

        public RowVersionTracker(string table = "RowVersionTracker")
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            this.table = table;
        }

        public async Task CreateTable(string connection, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await CreateTable(sqlConnection, cancellation).ConfigureAwait(false);
            }
        }

        public Task CreateTable(SqlConnection connection, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            return CreateTable(connection, null, cancellation);
        }

        public Task CreateTable(SqlTransaction transaction, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            return CreateTable(transaction.Connection, transaction, cancellation);
        }

        async Task CreateTable(SqlConnection sqlConnection, SqlTransaction transaction, CancellationToken cancellation)
        {
            using (var command = sqlConnection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = string.Format(Sql, table);
                await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
            }
        }

        public async Task Save(string connection, long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await Save(sqlConnection, null, rowVersion, cancellation).ConfigureAwait(false);
            }
        }

        async Task Save(SqlConnection connection, SqlTransaction transaction, long rowVersion, CancellationToken cancellation)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = $@"
update {table}
set RowVersion = @RowVersion
if @@rowcount = 0
    insert into {table} (RowVersion)
    values (@RowVersion)
";
                command.Parameters.Add("RowVersion", SqlDbType.BigInt).Value = rowVersion;
                await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
            }
        }

        public async Task<long> Get(string connection, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await Get(sqlConnection, cancellation);
            }
        }

        public async Task<long> Get(SqlConnection connection, CancellationToken cancellation = default)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"
select top (1) RowVersion
from {table}";
                var result = await command.ExecuteScalarAsync(cancellation).ConfigureAwait(false);
                if (result == null)
                {
                    return 1;
                }

                return (long) result;
            }
        }

        static string Sql = @"
if exists (
    select *
    from sys.objects
    where object_id = object_id('{0}')
        and type in ('U'))
return

create table {0} (
    RowVersion bigint not null
);
";

    }
}