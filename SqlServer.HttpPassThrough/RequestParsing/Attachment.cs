using System;
using System.IO;

namespace SqlHttpPassThrough
{
    public class Attachment
    {
        public Func<Stream> Stream;
        public string FileName;
    }
}