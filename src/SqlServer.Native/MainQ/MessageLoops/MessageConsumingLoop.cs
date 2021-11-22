﻿using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public class MessageConsumingLoop :
    MessageLoop
{
    string table;
    Func<CancellationToken, Task<SqlConnection>>? connectionBuilder;
    Func<CancellationToken, Task<SqlTransaction>>? transactionBuilder;
    Func<SqlTransaction, IncomingMessage, CancellationToken, Task>? transactionCallback;
    Func<SqlConnection, IncomingMessage, CancellationToken, Task>? connectionCallback;
    int batchSize;

    public MessageConsumingLoop(
        string table,
        Func<CancellationToken, Task<SqlTransaction>> transactionBuilder,
        Func<SqlTransaction, IncomingMessage, CancellationToken, Task> callback,
        Action<Exception> errorCallback,
        int batchSize = 10,
        TimeSpan? delay = null) :
        base(errorCallback, delay)
    {
        Guard.AgainstNullOrEmpty(table, nameof(table));
        Guard.AgainstNegativeAndZero(batchSize, nameof(batchSize));
        this.table = table;
        transactionCallback = callback.WrapFunc(nameof(transactionCallback));
        this.transactionBuilder = transactionBuilder.WrapFunc(nameof(transactionBuilder));
        this.batchSize = batchSize;
    }

    public MessageConsumingLoop(
        string table,
        Func<CancellationToken, Task<SqlConnection>> connectionBuilder,
        Func<SqlConnection, IncomingMessage, CancellationToken, Task> callback,
        Action<Exception> errorCallback,
        int batchSize = 10,
        TimeSpan? delay = null) :
        base(errorCallback, delay)
    {
        Guard.AgainstNullOrEmpty(table, nameof(table));
        Guard.AgainstNegativeAndZero(batchSize, nameof(batchSize));
        connectionCallback = callback.WrapFunc(nameof(connectionCallback));
        this.table = table;
        this.connectionBuilder = connectionBuilder.WrapFunc(nameof(this.connectionBuilder));
        this.batchSize = batchSize;
    }

    protected override async Task RunBatch(CancellationToken cancellation)
    {
        SqlConnection? connection = null;
        if (connectionBuilder != null)
        {
            await using (connection = await connectionBuilder(cancellation))
            {
                var consumer = new QueueManager(table, connection);
                await RunBatch(consumer, message => connectionCallback!(connection, message, cancellation), cancellation);
            }

            return;
        }
        SqlTransaction? transaction = null;
        try
        {
            transaction = await transactionBuilder!(cancellation);
            connection = transaction.Connection;
            var consumer = new QueueManager(table, transaction);
            try
            {
                await RunBatch(consumer, message => transactionCallback!(transaction, message, cancellation), cancellation);

                await transaction.CommitAsync(cancellation);
            }
            catch
            {
                await transaction.RollbackAsync(cancellation);
                throw;
            }
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }

            if (connection != null)
            {
                await connection.DisposeAsync();
            }
        }
    }

    async Task RunBatch(QueueManager consumer, Func<IncomingMessage, Task> action, CancellationToken cancellation)
    {
        while (true)
        {
            var result = await consumer.Consume(batchSize, action, cancellation);
            if (result.Count < batchSize)
            {
                break;
            }
        }
    }
}