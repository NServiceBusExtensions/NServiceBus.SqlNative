namespace NServiceBus.Transport.SqlServerNative;

/// <summary>
/// Handles creation of transport queues.
/// </summary>
public partial class QueueManager
{
    /// <summary>
    /// The sql statements used to create the queue.
    /// </summary>
    public override string CreateTableSql =>
        """
        if not exists (select * from sys.objects  where object_id = object_id('{0}') and type = 'U')
          create table {0} (
            Id uniqueidentifier not null,
            CorrelationId varchar(255),
            ReplyToAddress varchar(255),
            Recoverable bit not null,
            Expires datetime,
            Headers nvarchar(max) not null,{1}
            Body varbinary(max),
            RowVersion bigint identity(1,1) not null);

        -- drop any existing clustered index called Index_RowVersion
        if exists (select * from sys.indexes
          where object_id = object_id('{0}')
          and name = 'Index_RowVersion'
          and type = 1)

        drop index Index_RowVersion on {0}

        -- create nonclustered index Index_RowVersion if it doesn't exist
        if not exists (select * from sys.indexes
          where object_id = object_id('{0}')
          and name = 'Index_RowVersion'
          and type = 2)
        create nonclustered index Index_RowVersion on {0}
        (
          RowVersion
        )

        if not exists (select * from sys.indexes
                        where object_id = object_id('{0}')
                        and name = 'Index_Expires')
        create nonclustered index Index_Expires on {0}
        (
          Expires
        )
        include
        (
          Id,
          RowVersion
        )
        where Expires is not null
        """;
}