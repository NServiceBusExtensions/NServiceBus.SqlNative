﻿using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public class MessageProcessingLoop :
    MessageLoop
{
    string table;
    long startingRow;
    Func<CancellationToken, Task<SqlConnection>>? connectionBuilder;
    Func<CancellationToken, Task<SqlTransaction>>? transactionBuilder;
    Func<SqlTransaction, IncomingMessage, CancellationToken, Task>? transactionCallback;
    Func<SqlConnection, IncomingMessage, CancellationToken, Task>? connectionCallback;
    Func<SqlTransaction, long, CancellationToken, Task>? transactionPersistRowVersion;
    Func<SqlConnection, long, CancellationToken, Task>? connectionPersistRowVersion;
    int batchSize;

    public MessageProcessingLoop(
        string table,
        long startingRow,
        Func<CancellationToken, Task<SqlTransaction>> transactionBuilder,
        Func<SqlTransaction, IncomingMessage, CancellationToken, Task> callback,
        Action<Exception> errorCallback,
        Func<SqlTransaction, long, CancellationToken, Task> persistRowVersion,
        int batchSize = 10,
        TimeSpan? delay = null) :
        base(errorCallback, delay)
    {
        Guard.AgainstNullOrEmpty(table);
        Guard.AgainstNegativeAndZero(startingRow);
        Guard.AgainstNegativeAndZero(batchSize);
        this.table = table;
        this.startingRow = startingRow;
        this.transactionBuilder = transactionBuilder.WrapFunc(nameof(transactionBuilder));
        transactionCallback = callback.WrapFunc(nameof(transactionCallback));
        transactionPersistRowVersion = persistRowVersion;
        this.batchSize = batchSize;
    }

    public MessageProcessingLoop(
        string table,
        long startingRow,
        Func<CancellationToken, Task<SqlConnection>> connectionBuilder,
        Func<SqlConnection, IncomingMessage, CancellationToken, Task> callback,
        Action<Exception> errorCallback,
        Func<SqlConnection, long, CancellationToken, Task> persistRowVersion,
        int batchSize = 10,
        TimeSpan? delay = null) :
        base(errorCallback, delay)
    {
        Guard.AgainstNullOrEmpty(table);
        Guard.AgainstNegativeAndZero(startingRow);
        Guard.AgainstNegativeAndZero(batchSize);
        connectionCallback = callback.WrapFunc(nameof(connectionCallback));
        this.table = table;
        this.startingRow = startingRow;
        this.connectionBuilder = connectionBuilder.WrapFunc(nameof(connectionBuilder));
        connectionPersistRowVersion = persistRowVersion;
        this.batchSize = batchSize;
    }

    protected override async Task RunBatch(CancellationToken cancellation)
    {
        SqlConnection? connection = null;
        if (connectionBuilder != null)
        {
            using (connection = await connectionBuilder(cancellation))
            {
                var reader = new QueueManager(table, connection);
                await RunBatch(
                    reader,
                    messageFunc: message => connectionCallback!(connection, message, cancellation),
                    persistFunc: () => connectionPersistRowVersion!(connection, startingRow, cancellation),
                    cancellation);
            }

            return;
        }

        SqlTransaction? transaction = null;
        try
        {
            transaction = await transactionBuilder!(cancellation);
            connection = transaction.Connection;
            var reader = new QueueManager(table, transaction);
            try
            {
                await RunBatch(
                    reader,
                    messageFunc: message => transactionCallback!(transaction, message, cancellation),
                    persistFunc: () => transactionPersistRowVersion!(transaction, startingRow, cancellation),
                    cancellation);
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

    async Task RunBatch(QueueManager reader, Func<IncomingMessage, Task> messageFunc, Func<Task> persistFunc, CancellationToken cancellation)
    {
        while (true)
        {
            var result = await reader.Read(batchSize, startingRow, messageFunc, cancellation);
            if (result.Count == 0)
            {
                break;
            }

            startingRow = result.LastRowVersion.GetValueOrDefault(0) + 1;
            await persistFunc();
            if (result.Count < batchSize)
            {
                break;
            }
        }
    }
}