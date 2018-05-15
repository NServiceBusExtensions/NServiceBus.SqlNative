﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        public virtual Task<IncomingResult> ReadStream(int size, long startRowVersion, Action<IncomingStreamMessage> action, CancellationToken cancellation = default)
        {
            return ReadStream(size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> ReadStream(int size, long startRowVersion, Func<IncomingStreamMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNull(func, nameof(func));
            using (var command = BuildReadCommand(size, startRowVersion))
            {
                return await command.ReadMultipleStream(func, cancellation).ConfigureAwait(false);
            }
        }
    }
}