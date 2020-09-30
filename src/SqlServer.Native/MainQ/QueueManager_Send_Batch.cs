﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        public virtual async Task Send(IEnumerable<OutgoingMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(messages, nameof(messages));
            foreach (var message in messages)
            {
                await Send(message, cancellation);
            }
        }
    }
}