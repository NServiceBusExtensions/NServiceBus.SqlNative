using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

#if (SqlServerDedupe)
namespace NServiceBus.Transport.SqlServerDeduplication
#else
namespace NServiceBus.Transport.SqlServerNative
#endif
{
    static class ConnectionHelpers
    {
        internal static string WrapInNoCount(string sql)
        {
            return $@"
declare @nocount varchar(3) = 'off';
if ( (512 & @@options) = 512 ) set @nocount = 'on'
set nocount on;

{sql}

if (@nocount = 'on') set nocount on;
if (@nocount = 'off') set nocount off;";
        }

        internal static Task DropTable(this DbConnection connection, DbTransaction? transaction, Table table, CancellationToken cancellation = default)
        {
            return connection.RunCommand(transaction, $"drop table if exists {table}", cancellation);
        }
    }
}