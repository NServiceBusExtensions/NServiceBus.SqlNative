using System;
using System.Data;
using System.Data.SqlClient;
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

    //public static void AgainstSqlDelimiters(string argumentName, string value)
    //{
    //    if (value.Contains("]") || value.Contains("[") || value.Contains("`"))
    //    {
    //        throw new ArgumentException($"The argument '{value}' contains a ']', '[' or '`'. Names and schemas automatically quoted.");
    //    }
    //}

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

    public static Func<T, K> WrapFunc<T, K>(this Func<T, K> func, string name)
    {
        var exceptionMessage = $"Provided {name} delegate threw an exception.";
        var nullMessage = $"Provided {name} delegate returned a null.";
        return x =>
        {
            K value;
            try
            {
                value = func(x);
            }
            catch (Exception exception)
            {
                throw new Exception(exceptionMessage, exception);
            }

            if (value == null)
            {
                throw new Exception(nullMessage);
            }

            return value;
        };
    }

    public static Action<T> WrapFunc<T>(this Action<T> func, string name)
    {
        var exceptionMessage = $"Provided {name} delegate threw an exception.";
        return x =>
        {
            try
            {
                func(x);
            }
            catch (Exception exception)
            {
                throw new Exception(exceptionMessage, exception);
            }
        };
    }

    public static Action<T, K> WrapFunc<T, K>(this Action<T, K> func, string name)
    {
        var exceptionMessage = $"Provided {name} delegate threw an exception.";
        return (x, y) =>
        {
            try
            {
                func(x, y);
            }
            catch (Exception exception)
            {
                throw new Exception(exceptionMessage, exception);
            }
        };
    }

    public static Func<T1,T2,T3, Task> WrapFunc<T1, T2, T3>(this Func<T1, T2, T3, Task> func, string name)
    {
        var exceptionMessage = $"Provided {name} delegate threw an exception.";
        var nullMessage = $"Provided {name} delegate returned a null.";
        return async (x,y,z) =>
        {
            Task task;
            try
            {
                task = func(x, y, z);
            }
            catch (Exception exception)
            {
                throw new Exception(exceptionMessage, exception);
            }

            if (task == null)
            {
                throw new Exception(nullMessage);
            }

            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new Exception(exceptionMessage, exception);
            }
        };
    }
    public static Func<T, Task<K>> WrapFunc<T, K>(this Func<T, Task<K>> func, string name)
    {
        var exceptionMessage = $"Provided {name} delegate threw an exception.";
        var nullMessage = $"Provided {name} delegate returned a null.";
        return async x =>
        {
            Task<K> task;
            try
            {
                task = func(x);
            }
            catch (Exception exception)
            {
                throw new Exception(exceptionMessage, exception);
            }

            if (task == null)
            {
                throw new Exception(nullMessage);
            }

            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new Exception(exceptionMessage, exception);
            }
        };
    }
}