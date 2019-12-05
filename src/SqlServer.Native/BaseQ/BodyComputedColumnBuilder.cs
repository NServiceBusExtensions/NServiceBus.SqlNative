static class BodyComputedColumnBuilder
{
    public static string Computed(string? computedColumnSql)
    {
        if (computedColumnSql == null)
        {
            return @"
  BodyString as cast(Body as varchar(max)),";
        }

        return $@"
  BodyString as {computedColumnSql},";
    }
}