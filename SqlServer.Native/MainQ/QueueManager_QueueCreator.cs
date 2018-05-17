namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Handles creation of transport queues.
    /// </summary>
    public partial class QueueManager
    {
        /// <summary>
        /// The sql statements used to create the queue.
        /// </summary>
        public override string CreateTableSql => @"
if exists (
  select *
  from sys.objects
  where object_id = object_id('{0}')
    and type in ('U'))
return

create table {0} (
  Id uniqueidentifier not null,
  CorrelationId varchar(255),
  ReplyToAddress varchar(255),
  Recoverable bit not null,
  Expires datetime,
  Headers nvarchar(max) not null,{1}
  Body varbinary(max),
  RowVersion bigint identity(1,1) not null
);

create clustered index Index_RowVersion on {0}
(
  RowVersion
)

create nonclustered index Index_Expires on {0}
(
  Expires
)
include
(
  Id,
  RowVersion
)
where
  Expires is not null";
    }
}