using Headers = NServiceBus.Transport.SqlServerNative.Headers;

// ReSharper disable UnusedVariable

public class HeadersUsage
{
    static void Serialize()
    {
        #region Serialize

        var headers = new Dictionary<string, string>
        {
            {Headers.EnclosedMessageTypes, "SendMessage"}
        };
        var serialized = Headers.Serialize(headers);

        #endregion
    }

    static void Deserialize()
    {
        string headersString = null!;

        #region Deserialize

        var headers = Headers.DeSerialize(headersString);

        #endregion
    }
}