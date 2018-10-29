using System.Data.SqlClient;
using System.Text;

static class SqlExtensions
{
    static string quoteSuffix = "]";
    static string quotePrefix = "[";

    public static string Sanitize(string unquotedIdentifier)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(quotePrefix))
        {
            builder.Append(quotePrefix);
        }

        if (string.IsNullOrEmpty(quoteSuffix))
        {
            builder.Append(unquotedIdentifier);
        }
        else
        {
            builder.Append(unquotedIdentifier.Replace(quoteSuffix, quoteSuffix + quoteSuffix));
            builder.Append(quoteSuffix);
        }

        return builder.ToString();
    }

    public static bool IsKeyViolation(this SqlException sqlException)
    {
        foreach (SqlError sqlError in sqlException.Errors)
        {
            //Unique Key Violation = 2627
            if (sqlError.Number == 2627)
            {
                return true;
            }
        }

        return false;
    }
}