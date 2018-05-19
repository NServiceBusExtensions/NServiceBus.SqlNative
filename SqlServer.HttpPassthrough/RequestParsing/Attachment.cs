using System;
using System.IO;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    public class Attachment
    {
        public Func<Stream> Stream;
        public string FileName;
    }
}