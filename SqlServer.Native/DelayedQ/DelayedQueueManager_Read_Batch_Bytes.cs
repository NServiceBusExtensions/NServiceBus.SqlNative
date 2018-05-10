﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        public virtual Task<IncomingResult> ReadBytes(int size, long startRowVersion, Action<IncomingDelayedBytesMessage> action, CancellationToken cancellation = default)
        {
            return ReadBytes(size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> ReadBytes(int size, long startRowVersion, Func<IncomingDelayedBytesMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNull(func, nameof(func));
            using (var command = BuildReadCommand(size, startRowVersion))
            {
                return await command.ReadDelayedMultipleBytes(func, cancellation);
            }
        }
    }
}