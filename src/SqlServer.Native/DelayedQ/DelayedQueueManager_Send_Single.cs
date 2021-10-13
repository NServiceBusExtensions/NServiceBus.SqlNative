using System.Data;
using System.Data.Common;

namespace NServiceBus.Transport.SqlServerNative;

public partial class DelayedQueueManager
{
    protected override DbCommand CreateSendCommand(OutgoingDelayedMessage message)
    {
        var command = Connection.CreateCommand(Transaction, string.Format(SendSql, Table));

        var dueParameter = CreateDueParameter(command);
        var headersParameter = CreateHeadersParameter(command);
        var bodyParameter = CreateBodyParameter(command);

        dueParameter.Value = message.Due;
        headersParameter.Value = message.Headers;
        bodyParameter.SetBinaryOrDbNull(message.Body);

        return command;
    }

    static DbParameter CreateDueParameter(DbCommand command)
    {
        var dueParameter = command.CreateParameter();
        dueParameter.ParameterName = "Due";
        dueParameter.DbType = DbType.DateTime;
        command.Parameters.Add(dueParameter);
        return dueParameter;
    }

    static DbParameter CreateHeadersParameter(DbCommand command)
    {
        var headersParameter = command.CreateParameter();
        headersParameter.ParameterName = "Headers";
        headersParameter.DbType = DbType.String;
        command.Parameters.Add(headersParameter);
        return headersParameter;
    }

    static DbParameter CreateBodyParameter(DbCommand command)
    {
        var bodyParameter = command.CreateParameter();
        bodyParameter.ParameterName = "Body";
        bodyParameter.DbType = DbType.Binary;
        command.Parameters.Add(bodyParameter);
        return bodyParameter;
    }
}