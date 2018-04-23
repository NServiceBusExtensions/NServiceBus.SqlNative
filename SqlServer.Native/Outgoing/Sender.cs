namespace SqlServer.Native
{
    public partial class Sender
    {
        string table;

        public Sender(string table)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            this.table = table;
        }

        public static readonly string Sql =
            @"
declare @nocount varchar(3) = 'off';
if ( (512 & @@options) = 512 ) set @nocount = 'on'
set nocount on;

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
    @Body);

if (@nocount = 'on') set nocount on;
if (@nocount = 'off') set nocount off;";
    }
}