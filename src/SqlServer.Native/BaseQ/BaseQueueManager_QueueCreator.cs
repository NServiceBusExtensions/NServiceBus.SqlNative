﻿namespace NServiceBus.Transport.SqlServerNative;

public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
    where TIncoming : class, IIncomingMessage
{
    /// <summary>
    /// Creates a queue.
    /// </summary>
    public virtual Task Create(string computedColumnSql, Cancellation cancellation = default)
    {
        Guard.AgainstNullOrEmpty(computedColumnSql);
        return InnerCreate(true, computedColumnSql, cancellation);
    }

    /// <summary>
    /// Creates a queue.
    /// </summary>
    public virtual Task Create(bool createDecodedBodyComputedColumn = true, Cancellation cancellation = default) =>
        InnerCreate(createDecodedBodyComputedColumn, null, cancellation);

    /// <summary>
    /// Drops a queue.
    /// </summary>
    public virtual Task Drop(Cancellation cancellation = default) =>
        Connection.DropTable(Transaction, Table, cancellation);

    Task InnerCreate(bool createDecodedBodyComputedColumn, string? computedColumnSql, Cancellation cancellation)
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
        return Connection.RunCommand(Transaction, commandText, cancellation);
    }

    /// <summary>
    /// The sql statements used to create the queue.
    /// </summary>
    public abstract string CreateTableSql { get; }
}