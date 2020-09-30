using System.Data;
using System.Data.Common;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        protected override void PopulateSendCommand(DbCommand command, OutgoingMessage message)
        {
            var parameters = command.Parameters;

            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "Id";
            idParameter.DbType = DbType.Guid;
            idParameter.Value = message.Id;
            parameters.Add(idParameter);

            var expiresParameter = command.CreateParameter();
            expiresParameter.ParameterName = "Expires";
            expiresParameter.DbType = DbType.DateTime;
            expiresParameter.SetValueOrDbNull(message.Expires);
            parameters.Add(expiresParameter);

            var headersParameter = command.CreateParameter();
            headersParameter.ParameterName = "Headers";
            headersParameter.DbType = DbType.String;
            headersParameter.Value = message.Headers;
            parameters.Add(headersParameter);

            var bodyParameter = command.CreateParameter();
            bodyParameter.ParameterName = "Body";
            bodyParameter.DbType = DbType.Binary;
            bodyParameter.SetBinaryOrDbNull(message.Body);
            parameters.Add(bodyParameter);
        }

        protected override DbCommand CreateSendCommand()
        {
            return Connection.CreateCommand(Transaction, string.Format(sendSql, Table));
        }
    }
}