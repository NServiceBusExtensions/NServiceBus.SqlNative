namespace SqlServer.Native
{
    public partial class DelayedSender
    {
        string table;

        public DelayedSender(string table)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            this.table = table;
        }

        public static readonly string Sql = SqlHelpers.WrapInNoCount(
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