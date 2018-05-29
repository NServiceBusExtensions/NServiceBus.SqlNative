using System.Data;
using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        protected override SqlCommand CreateSendCommand(OutgoingDelayedMessage message)
        {
            var command = Connection.CreateCommand(Transaction, string.Format(SendSql, Table));
            var parameters = command.Parameters;
            parameters.Add("Due", SqlDbType.DateTime).Value = message.Due;
            parameters.Add("Headers", SqlDbType.NVarChar).Value = message.Headers;
            parameters.Add("Body", SqlDbType.VarBinary).SetBinaryOrDbNull(message.Body);
            return command;
        }
    }
}