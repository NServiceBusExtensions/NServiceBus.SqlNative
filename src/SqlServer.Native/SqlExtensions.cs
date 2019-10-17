using System.Data.Common;
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

    public static bool IsKeyViolation(this DbException sqlException)
    {
        var exception = (dynamic) sqlException;
        //Unique Key Violation = 2627
        return exception.Number == 2627;
    }
}