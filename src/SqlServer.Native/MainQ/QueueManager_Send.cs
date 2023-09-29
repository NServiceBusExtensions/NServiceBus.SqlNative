namespace NServiceBus.Transport.SqlServerNative;

public partial class QueueManager
{
    string sendSql = null!;

    const string dedupSql =
        """
        if exists (
          select *
          from {0}
          where Id = @Id)
        return

        insert into {0} (Id)
        values (@Id);
        """;

    const string sql =
        """
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
          @Body);
        """;

    void InitSendSql()
    {
        string resultSql;
        if (dedupe)
        {
            resultSql = string.Format(dedupSql, dedupeTable) + string.Format(sql, Table);
        }
        else
        {
            resultSql = string.Format(sql, Table);
        }

        sendSql = ConnectionHelpers.WrapInNoCount(resultSql);
    }
}