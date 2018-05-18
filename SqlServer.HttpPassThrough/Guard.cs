using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

static class Guard
{
    // ReSharper disable UnusedParameter.Global
    public static void AgainstNull(object value, string argumentName)
    {
        if (value == null)
        {
            throw new ArgumentNullException(argumentName);
        }
    }
    public static void AgainstNull(SqlConnection value, string argumentName)
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

    public static void AgainstSqlDelimiters(string argumentName, string value)
    {
        if (value.Contains("]") || value.Contains("[") || value.Contains("`"))
        {
            throw new ArgumentException($"The argument '{value}' contains a ']', '[' or '`'. Names and schemas automatically quoted.");
        }
    }

    public static void AgainstNullOrEmpty(string value, string argumentName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstEmpty(string value, string argumentName)
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
            throw new ArgumentException("Value cannot be empty.", argumentName);
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
        if (value<1)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstNegativeAndZero(long value, string argumentName)
    {
        if (value<1)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static Func<T> WrapFuncInCheck<T>(this Func<T> func, string name)
    {
        return () => func.EvaluateAndCheck(name);
    }

    static T EvaluateAndCheck<T>(this Func<T> func, string attachmentName)
    {
        var message = $"Provided delegate threw an exception. Attachment name: {attachmentName}.";
        T value;
        try
        {
            value = func();
        }
        catch (Exception exception)
        {
            throw new Exception(message, exception);
        }

        ThrowIfNullReturned(null, attachmentName, value);
        return value;
    }

    public static Action WrapCleanupInCheck(this Action cleanup, string attachmentName)
    {
        if (cleanup == null)
        {
            return null;
        }

        return () =>
        {
            try
            {
                cleanup();
            }
            catch (Exception exception)
            {
                throw new Exception($"Cleanup threw an exception. Attachment name: {attachmentName}.", exception);
            }
        };
    }

    public static Func<Task<T>> WrapFuncTaskInCheck<T>(this Func<Task<T>> func, string attachmentName)
    {
        return async () =>
        {
            var task = func.EvaluateAndCheck(attachmentName);
            ThrowIfNullReturned(null, attachmentName, task);
            var value = await task.ConfigureAwait(false);
            ThrowIfNullReturned(null, attachmentName, value);
            return value;
        };
    }

    public static Func<Task<Stream>> WrapStreamFuncTaskInCheck<T>(this Func<Task<T>> func, string attachmentName)
        where T : Stream
    {
        return async () =>
        {
            var task = func.EvaluateAndCheck(attachmentName);
            ThrowIfNullReturned(null, attachmentName, task);
            var value = await task.ConfigureAwait(false);
            ThrowIfNullReturned(null,attachmentName, value);
            return value;
        };
    }

    public static void ThrowIfNullReturned(string messageId, string attachmentName, object value)
    {
        if (value == null)
        {
            if (attachmentName != null && messageId != null)
            {
                throw new Exception($"Provided delegate returned a null. MessageId: '{messageId}', Attachment: '{attachmentName}'.");
            }

            if (attachmentName != null)
            {
                throw new Exception($"Provided delegate returned a null. Attachment: '{attachmentName}'.");
            }

            if (messageId != null)
            {
                throw new Exception($"Provided delegate returned a null. MessageId: '{messageId}'.");
            }

            throw new Exception("Provided delegate returned a null.");
        }
    }
}