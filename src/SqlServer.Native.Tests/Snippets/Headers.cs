using NServiceBus.Transport.SqlServerNative;
// ReSharper disable UnusedVariable

public class HeadersUsage
{
    void Serialize()
    {
        #region Serialize

        var headers = new Dictionary<string, string>
        {
            {Headers.EnclosedMessageTypes, "SendMessage"}
        };
        var serialized = Headers.Serialize(headers);

        #endregion
    }

    void Deserialize()
    {
        string headersString = null!;

        #region Deserialize

        var headers = Headers.DeSerialize(headersString);

        #endregion
    }
}