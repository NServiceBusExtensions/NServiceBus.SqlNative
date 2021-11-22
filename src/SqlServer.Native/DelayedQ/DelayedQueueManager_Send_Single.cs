using System.Data;
using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public partial class DelayedQueueManager
{
    protected override SqlCommand CreateSendCommand(OutgoingDelayedMessage message)
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

    static SqlParameter CreateDueParameter(SqlCommand command)
    {
        var dueParameter = command.CreateParameter();
        dueParameter.ParameterName = "Due";
        dueParameter.DbType = DbType.DateTime;
        command.Parameters.Add(dueParameter);
        return dueParameter;
    }

    static SqlParameter CreateHeadersParameter(SqlCommand command)
    {
        var headersParameter = command.CreateParameter();
        headersParameter.ParameterName = "Headers";
        headersParameter.DbType = DbType.String;
        command.Parameters.Add(headersParameter);
        return headersParameter;
    }

    static SqlParameter CreateBodyParameter(SqlCommand command)
    {
        var bodyParameter = command.CreateParameter();
        bodyParameter.ParameterName = "Body";
        bodyParameter.DbType = DbType.Binary;
        command.Parameters.Add(bodyParameter);
        return bodyParameter;
    }
}