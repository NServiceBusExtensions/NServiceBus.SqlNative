using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public class MessageProcessingLoop :
    MessageLoop
{
    string table;
    long startingRow;
    Func<Cancellation, Task<SqlConnection>>? connectionBuilder;
    Func<Cancellation, Task<SqlTransaction>>? transactionBuilder;
    Func<SqlTransaction, IncomingMessage, Cancellation, Task>? transactionCallback;
    Func<SqlConnection, IncomingMessage, Cancellation, Task>? connectionCallback;
    Func<SqlTransaction, long, Cancellation, Task>? transactionPersistRowVersion;
    Func<SqlConnection, long, Cancellation, Task>? connectionPersistRowVersion;
    int batchSize;

    public MessageProcessingLoop(
        string table,
        long startingRow,
        Func<Cancellation, Task<SqlTransaction>> transactionBuilder,
        Func<SqlTransaction, IncomingMessage, Cancellation, Task> callback,
        Action<Exception> errorCallback,
        Func<SqlTransaction, long, Cancellation, Task> persistRowVersion,
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
        Func<Cancellation, Task<SqlConnection>> connectionBuilder,
        Func<SqlConnection, IncomingMessage, Cancellation, Task> callback,
        Action<Exception> errorCallback,
        Func<SqlConnection, long, Cancellation, Task> persistRowVersion,
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

    protected override async Task RunBatch(Cancellation cancellation)
    {
        SqlConnection? connection = null;
        if (connectionBuilder != null)
        {
            using (connection = await connectionBuilder(cancellation))
            {
                var reader = new QueueManager(table, connection);
                await RunBatch(
                    reader,
                    messageFunc: (message, cancellation) => connectionCallback!(connection, message, cancellation),
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
                    messageFunc: (message, cancellation) => transactionCallback!(transaction, message, cancellation),
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

    async Task RunBatch(QueueManager reader, Func<IncomingMessage, Cancellation, Task> messageFunc, Func<Task> persistFunc, Cancellation cancellation)
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