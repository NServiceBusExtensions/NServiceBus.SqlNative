static class Guard
{
    public static void AgainstNullOrEmpty(string value, [CallerArgumentExpression("value")] string argumentName = "")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstEmpty(string? value, [CallerArgumentExpression("value")] string argumentName = "")
    {
        if (value == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstEmpty(Guid value, [CallerArgumentExpression("value")] string argumentName = "")
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstNegativeAndZero(TimeSpan? value, [CallerArgumentExpression("value")] string argumentName = "")
    {
        if (value == null)
        {
            return;
        }

        if (value < TimeSpan.Zero || value < TimeSpan.Zero)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstNegativeAndZero(int value, [CallerArgumentExpression("value")] string argumentName = "")
    {
        if (value < 1)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstNegativeAndZero(long value, [CallerArgumentExpression("value")] string argumentName = "")
    {
        if (value < 1)
        {
            throw new ArgumentNullException(argumentName);
        }
    }
}