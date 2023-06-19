namespace NServiceBus.Transport.SqlServerNative;

public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
    where TIncoming : class, IIncomingMessage
{
    /// <summary>
    /// Creates a queue.
    /// </summary>
    public virtual Task Create(string computedColumnSql, Cancel cancel = default)
    {
        Guard.AgainstNullOrEmpty(computedColumnSql);
        return InnerCreate(true, computedColumnSql, cancel);
    }

    /// <summary>
    /// Creates a queue.
    /// </summary>
    public virtual Task Create(bool createDecodedBodyComputedColumn = true, Cancel cancel = default) =>
        InnerCreate(createDecodedBodyComputedColumn, null, cancel);

    /// <summary>
    /// Drops a queue.
    /// </summary>
    public virtual Task Drop(Cancel cancel = default) =>
        Connection.DropTable(Transaction, Table, cancel);

    Task InnerCreate(bool createDecodedBodyComputedColumn, string? computedColumnSql, Cancel cancel)
    {
        if (createDecodedBodyComputedColumn)
        {
            computedColumnSql = BodyComputedColumnBuilder.Computed(computedColumnSql);
        }
        else
        {
            computedColumnSql = string.Empty;
        }

        var commandText = string.Format(CreateTableSql, Table, computedColumnSql);
        return Connection.RunCommand(Transaction, commandText, cancel);
    }

    /// <summary>
    /// The sql statements used to create the queue.
    /// </summary>
    public abstract string CreateTableSql { get; }
}