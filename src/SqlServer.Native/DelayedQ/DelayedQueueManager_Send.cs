namespace NServiceBus.Transport.SqlServerNative;

public partial class DelayedQueueManager
{
    public static readonly string SendSql = ConnectionHelpers.WrapInNoCount(
        """
        insert into {0} (
          Due,
          Headers,
          Body)
        output inserted.RowVersion
        values (
          @Due,
          @Headers,
          @Body);
        """);
}