using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

static class TransactionWrapper
{
    public static async Task<T> Run<T>(Func<SqlTransaction, Task<T>> func, SqlConnection connection, SqlTransaction transaction=null)
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