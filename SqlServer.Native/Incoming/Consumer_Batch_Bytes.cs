using System;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Consumer
    {
        public virtual Task<IncomingResult> ConsumeBytes(int size, Action<IncomingBytesMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            return ConsumeBytes(size, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> ConsumeBytes(int size, Func<IncomingBytesMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(func, nameof(func));
            using (var command = BuildCommand(size))
            {
                return await command.ReadMultipleBytes(func, cancellation);
            }
        }
    }
}