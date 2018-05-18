namespace NServiceBus.Transport.SqlServerNative
{
    public class Table
    {
        public Table(string tableName, string schema = "dbo") :
            this(tableName, schema, true)
        {
        }

        public Table(string tableName, string schema, bool sanitize)
        {
            Guard.AgainstNullOrEmpty(tableName, nameof(tableName));
            Guard.AgainstNullOrEmpty(schema, nameof(schema));
            if (sanitize)
            {
                TableName = SqlSanitizer.Sanitize(tableName);
                Schema = SqlSanitizer.Sanitize(schema);
            }

            FullTableName = $"{Schema}.{TableName}";
        }

        public string FullTableName { get; }
        public string TableName { get; }
        public string Schema { get; }

        public static implicit operator Table(string table)
        {
            return new Table(table);
        }

        public override string ToString()
        {
            return FullTableName;
        }
    }
}