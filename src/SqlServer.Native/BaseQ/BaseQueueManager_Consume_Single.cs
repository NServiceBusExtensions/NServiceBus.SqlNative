﻿using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
    where TIncoming : class, IIncomingMessage
{
    public virtual async Task<TIncoming?> Consume(Cancel cancel = default)
    {
        var shouldCleanup = false;
        SqlDataReader? reader = null;
        try
        {
            using var command = BuildConsumeCommand(1);
            reader = await command.RunSingleRowReader(cancel);
            if (!await reader.ReadAsync(cancel))
            {
                reader.Dispose();
                return default;
            }

            return await ReadMessage(reader, reader.DisposeAsync);
        }
        catch
        {
            shouldCleanup = true;
            throw;
        }
        finally
        {
            if (shouldCleanup && reader != null)
            {
                reader.Dispose();
            }
        }
    }
}