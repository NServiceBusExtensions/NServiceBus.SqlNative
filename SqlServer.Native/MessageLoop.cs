using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{


    public class MessageLoop
    {
        public MessageLoop(
            Func<Task<SqlConnection>> connectionBuilder,
            long startRowVersion,
            string table,
            Func<long,IList<Message>, CancellationToken, Task> callback,
            TimeSpan interval,
            Action<Exception> errorCallback)
        {

            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            long rowVersion = startRowVersion;
            var finder = new Finder(table);
            var size = 10;
            task = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var connection = await connectionBuilder().ConfigureAwait(false);
                        var messages = new List<Message>();
                        var count = await finder.Find(connection, size, rowVersion,
                                message => messages.Add(message),
                                token)
                            .ConfigureAwait(false);
                        rowVersion += count;
                        await callback(rowVersion, messages, token).ConfigureAwait(false);
                        if (count < size)
                        {
                            await Task.Delay(interval, token).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // noop
                    }
                    catch (Exception ex)
                    {
                        errorCallback(ex);
                    }
                }
            }, CancellationToken.None);
        }

        public Task Stop()
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
            return task;
        }

        Task task;
        CancellationTokenSource tokenSource;
    }
}