using System.Text;

static class JsonConvert
{
    internal static string ToString(string type)
    {
        return EscapeToJavascriptString(type);
    }
    public static string EscapeToJavascriptString(string jsonString)
    {
        var builder = new StringBuilder();

        for (var i = 0; i < jsonString.Length;)
        {
            var c = jsonString[i++];

            if (c != '\\')
            {
                builder.Append(c);
                continue;
            }

            var remainingLength = jsonString.Length - i;
            if (remainingLength < 2)
            {
                continue;
            }

            var lookahead = jsonString[i];
            if (lookahead == '\\')
            {
                builder.Append('\\');
                ++i;
                continue;
            }

            if (lookahead == '"')
            {
                builder.Append("\"");
                ++i;
                continue;
            }

            if (lookahead == 't')
            {
                builder.Append('\t');
                ++i;
                continue;
            }

            if (lookahead == 'b')
            {
                builder.Append('\b');
                ++i;
                continue;
            }

            if (lookahead == 'n')
            {
                builder.Append('\n');
                ++i;
                continue;
            }

            if (lookahead == 'r')
            {
                builder.Append('\r');
                ++i;
            }
        }
        return builder.ToString();
    }
}