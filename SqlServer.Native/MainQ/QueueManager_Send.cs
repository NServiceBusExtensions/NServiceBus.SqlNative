namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        string sendSql;

        void InitSendSql()
        {
            const string dedupSql = @"
if exists (
    select *
    from {0}
    where Id = @Id)
return

insert into {0} (Id)
values (@Id);";
            const string sql = @"
insert into {0} (
    Id,
    Recoverable,
    Expires,
    Headers,
    Body)
output inserted.RowVersion
values (
    @Id,
    1,
    @Expires,
    @Headers,
    @Body);";

            string resultSql;
            if (deduplicate)
            {
                resultSql = string.Format(dedupSql, deduplicationTable) + string.Format(sql, table);
            }
            else
            {
                resultSql = string.Format(sql, table);
            }

            sendSql = ConnectionHelpers.WrapInNoCount(resultSql);
        }
    }
}