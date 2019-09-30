using System;
using System.Data;
using System.Data.Common;

static class Guard
{
    // ReSharper disable UnusedParameter.Global
    public static void AgainstNull(object? value, string argumentName)
    {
        if (value == null)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstNull(DbConnection? value, string argumentName)
    {
        if (value == null)
        {
            throw new ArgumentNullException(argumentName);
        }

        if (value.State == ConnectionState.Closed)
        {
            throw new ArgumentException("Connection must be open.", argumentName);
        }
    }

    public static void AgainstNullOrEmpty(string? value, string argumentName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstEmpty(string? value, string argumentName)
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

    public static void AgainstEmpty(Guid value, string argumentName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstNegativeAndZero(TimeSpan? value, string argumentName)
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

    public static void AgainstNegativeAndZero(int value, string argumentName)
    {
        if (value < 1)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstNegativeAndZero(long value, string argumentName)
    {
        if (value < 1)
        {
            throw new ArgumentNullException(argumentName);
        }
    }
}