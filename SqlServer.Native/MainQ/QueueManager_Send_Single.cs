using System.Data;
using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        protected override SqlCommand CreateSendCommand(OutgoingMessage message)
        {
            var command = Connection.CreateCommand(Transaction, string.Format(sendSql, Table));
            var parameters = command.Parameters;
            parameters.Add("Id", SqlDbType.UniqueIdentifier).Value = message.Id;
            parameters.Add("Expires", SqlDbType.DateTime).SetValueOrDbNull(message.Expires);
            parameters.Add("Headers", SqlDbType.NVarChar).Value = message.Headers;
            parameters.Add("Body", SqlDbType.VarBinary).SetBinaryOrDbNull(message.Body);
            return command;
        }
    }
}