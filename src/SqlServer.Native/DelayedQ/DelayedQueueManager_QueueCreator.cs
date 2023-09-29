namespace NServiceBus.Transport.SqlServerNative;

public partial class DelayedQueueManager
{
    /// <summary>
    /// The sql statements used to create the Delayed queue.
    /// </summary>
    public override string CreateTableSql =>
        """
        if exists (
          select *
          from sys.objects
          where object_id = object_id('{0}')
            and type in ('U'))
        return

        create table {0} (
          Headers nvarchar(max) not null,{1}
          Body varbinary(max),
          Due datetime not null,
          RowVersion bigint identity(1,1) not null
        );

        create nonclustered index [Index_Due] on {0}
        (
          [Due]
        )
        """;
}