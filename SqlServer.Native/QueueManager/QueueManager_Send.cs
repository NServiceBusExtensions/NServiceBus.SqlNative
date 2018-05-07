namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        string sendSql;

        void InitSendSql()
        {
            const string dedupsql = @"

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
    CorrelationId,
    ReplyToAddress,
    Recoverable,
    Expires,
    Headers,
    Body)
output inserted.RowVersion
values (
    @Id,
    @CorrelationId,
    @ReplyToAddress,
    1,
    @Expires,
    @Headers,
    @Body);";

            string resultSql;
            if (deduplicate)
            {
                resultSql = string.Format(dedupsql, deduplicationTable) + string.Format(sql, table);
            }
            else
            {
                resultSql = string.Format(sql, table);
            }

            sendSql = ConnectionHelpers.WrapInNoCount(resultSql);
        }
    }
}