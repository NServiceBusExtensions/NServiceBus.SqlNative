namespace SqlServer.Native
{
    public struct IncomingResult
    {
        public long? LastRowVersion { get; set; }
        public int Count { get; set; }
    }
}