namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Sender
    {
        string table;

        public Sender(string table)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            this.table = table;
        }

        public static readonly string Sql = SqlHelpers.WrapInNoCount(
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