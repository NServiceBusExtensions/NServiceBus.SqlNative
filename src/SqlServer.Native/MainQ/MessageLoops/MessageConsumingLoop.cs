﻿using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public class MessageConsumingLoop :
    MessageLoop
{
    string table;
    Func<Cancellation, Task<SqlConnection>>? connectionBuilder;
    Func<Cancellation, Task<SqlTransaction>>? transactionBuilder;
    Func<SqlTransaction, IncomingMessage, Cancellation, Task>? transactionCallback;
    Func<SqlConnection, IncomingMessage, Cancellation, Task>? connectionCallback;
    int batchSize;

    public MessageConsumingLoop(
        string table,
        Func<Cancellation, Task<SqlTransaction>> transactionBuilder,
        Func<SqlTransaction, IncomingMessage, Cancellation, Task> callback,
        Action<Exception> errorCallback,
        int batchSize = 10,
        TimeSpan? delay = null) :
        base(errorCallback, delay)
    {
        Guard.AgainstNullOrEmpty(table);
        Guard.AgainstNegativeAndZero(batchSize);
        this.table = table;
        transactionCallback = callback.WrapFunc(nameof(transactionCallback));
        this.transactionBuilder = transactionBuilder.WrapFunc(nameof(transactionBuilder));
        this.batchSize = batchSize;
    }

    public MessageConsumingLoop(
        string table,
        Func<Cancellation, Task<SqlConnection>> connectionBuilder,
        Func<SqlConnection, IncomingMessage, Cancellation, Task> callback,
        Action<Exception> errorCallback,
        int batchSize = 10,
        TimeSpan? delay = null) :
        base(errorCallback, delay)
    {
        Guard.AgainstNullOrEmpty(table);
        Guard.AgainstNegativeAndZero(batchSize);
        connectionCallback = callback.WrapFunc(nameof(connectionCallback));
        this.table = table;
        this.connectionBuilder = connectionBuilder.WrapFunc(nameof(this.connectionBuilder));
        this.batchSize = batchSize;
    }

    protected override async Task RunBatch(Cancellation cancellation)
    {
        SqlConnection? connection = null;
        if (connectionBuilder != null)
        {
            using (connection = await connectionBuilder(cancellation))
            {
                var consumer = new QueueManager(table, connection);
                await RunBatch(consumer, (message, cancellation) => connectionCallback!(connection, message, cancellation), cancellation);
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
                await RunBatch(consumer, (message, cancellation) => transactionCallback!(transaction, message, cancellation), cancellation);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        finally
        {
            transaction?.Dispose();

            connection?.Dispose();
        }
    }

    async Task RunBatch(QueueManager consumer, Func<IncomingMessage, Cancellation, Task> action, Cancellation cancellation)
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