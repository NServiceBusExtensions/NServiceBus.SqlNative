namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        public static readonly string SendSql = SqlHelpers.WrapInNoCount(
            @"
insert into {0} (
    Due,
    Headers,
    Body)
output inserted.RowVersion
values (
    @Due,
    @Headers,
    @Body);");
    }
}