using System.Data;
using System.Data.Common;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        protected override DbCommand CreateSendCommand(OutgoingDelayedMessage message)
        {
            var command = Connection.CreateCommand(Transaction, string.Format(SendSql, Table));
            
            var dueParameter = command.CreateParameter();
            dueParameter.ParameterName = "Due";
            dueParameter.DbType = DbType.DateTime;
            dueParameter.Value = message.Due;
            command.Parameters.Add(dueParameter);

            var headersParameter = command.CreateParameter();
            headersParameter.ParameterName = "Headers";
            headersParameter.DbType = DbType.String;
            headersParameter.Value = message.Headers;
            command.Parameters.Add(headersParameter);

            var bodyParameter = command.CreateParameter();
            bodyParameter.ParameterName = "Body";
            bodyParameter.DbType = DbType.Binary;
            bodyParameter.SetBinaryOrDbNull(message.Body);
            command.Parameters.Add(bodyParameter);

            return command;
        }
    }
}