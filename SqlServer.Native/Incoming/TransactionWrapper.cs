using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

static class TransactionWrapper
{
    public static async Task<T> Run<T>(SqlConnection connection, SqlTransaction transaction, Func<SqlTransaction, Task<T>> func)
    {
        var ownsTransaction = false;
        if (transaction == null)
        {
            ownsTransaction = true;
            transaction = connection.BeginTransaction();
        }

        try
        {
            var incomingResult = await func(transaction).ConfigureAwait(false);
            if (ownsTransaction)
            {
                transaction.Commit();
            }

            return incomingResult;
        }
        catch (Exception)
        {
            if (ownsTransaction)
            {
                transaction.Rollback();
            }

            throw;
        }
        finally
        {
            if (ownsTransaction)
            {
                transaction.Dispose();
            }
        }
    }
}