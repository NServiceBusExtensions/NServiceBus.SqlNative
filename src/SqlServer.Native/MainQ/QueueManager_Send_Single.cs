using System.Data;
using System.Data.Common;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        protected override DbCommand CreateSendCommand(OutgoingMessage message)
        {
            var command = Connection.CreateCommand(Transaction, string.Format(sendSql, Table));
            var parameters = command.Parameters;

            var idParameter = CreateIdParameter(command, parameters);
            var expiresParameter = CreateExpiresParameter(command, parameters);
            var headersParameter = CreateHeadersParameter(command, parameters);
            var bodyParameter = CreateBodyParameter(command, parameters);

            idParameter.Value = message.Id;
            expiresParameter.SetValueOrDbNull(message.Expires);
            headersParameter.Value = message.Headers;
            bodyParameter.SetBinaryOrDbNull(message.Body);

            return command;
        }

        static DbParameter CreateIdParameter(DbCommand command, DbParameterCollection parameters)
        {
            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "Id";
            idParameter.DbType = DbType.Guid;
            parameters.Add(idParameter);
            return idParameter;
        }

        static DbParameter CreateExpiresParameter(DbCommand command, DbParameterCollection parameters)
        {
            var expiresParameter = command.CreateParameter();
            expiresParameter.ParameterName = "Expires";
            expiresParameter.DbType = DbType.DateTime;
            parameters.Add(expiresParameter);
            return expiresParameter;
        }

        static DbParameter CreateHeadersParameter(DbCommand command, DbParameterCollection parameters)
        {
            var headersParameter = command.CreateParameter();
            headersParameter.ParameterName = "Headers";
            headersParameter.DbType = DbType.String;
            parameters.Add(headersParameter);
            return headersParameter;
        }

        static DbParameter CreateBodyParameter(DbCommand command, DbParameterCollection parameters)
        {
            var bodyParameter = command.CreateParameter();
            bodyParameter.ParameterName = "Body";
            bodyParameter.DbType = DbType.Binary;
            parameters.Add(bodyParameter);
            return bodyParameter;
        }
    }
}