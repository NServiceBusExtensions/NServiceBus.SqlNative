using System.Data;
using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        protected override SqlCommand CreateSendCommand(OutgoingDelayedMessage message)
        {
            var command = connection.CreateCommand(transaction, string.Format(SendSql, table));
            var parameters = command.Parameters;
            parameters.Add("Due", SqlDbType.DateTime).Value = message.Due;
            parameters.Add("Headers", SqlDbType.NVarChar).Value = message.Headers;
            parameters.Add("Body", SqlDbType.VarBinary).SetBinaryOrDbNull(message.Body);
            return command;
        }
    }
}