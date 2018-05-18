using System;
using System.IO;

namespace NServiceBus.SqlServer.HttpPassThrough
{
    public class Attachment
    {
        public Func<Stream> Stream;
        public string FileName;
    }
}