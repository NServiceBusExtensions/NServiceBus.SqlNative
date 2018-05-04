namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        public static readonly string SendSql = ConnectionHelpers.WrapInNoCount(
            @"
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
    @Body);");
    }
}