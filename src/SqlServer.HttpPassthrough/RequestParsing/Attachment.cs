using NServiceBus.Attachments.Sql.Raw;

namespace NServiceBus.SqlServer.HttpPassthrough;

/// <summary>
/// The attachment part of a <see cref="PassthroughMessage"/>.
/// </summary>
public class Attachment
{
    public Attachment(Func<Stream> stream, string fileName)
    {
        Guard.AgainstNullOrEmpty(fileName);
        Stream = stream;
        FileName = fileName;
    }

    /// <summary>
    /// A delegate that returns the instance of the <see cref="Stream"/> to send.
    /// The resulting <see cref="Stream"/> will be passed to 'stream' parameter of <see cref="Persister.SaveStream"/>.
    /// </summary>
    public Func<Stream> Stream { get; }

    /// <summary>
    /// The file name of the <see cref="Attachment"/>.
    /// Will be passed to 'name' parameter of <see cref="Persister.SaveStream"/>.
    /// </summary>
    public string FileName { get; }
}