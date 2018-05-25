using System;
using System.Threading.Tasks;

static class DelegateWrappers
{
    static string threwAnException = "Provided {0} delegate threw an exception.";
    static string returnedNull = "Provided {0} delegate returned a null.";

    public static Func<T, K> WrapFunc<T, K>(this Func<T, K> func, string name)
    {
        var exceptionMessage = string.Format(threwAnException, name);
        var nullMessage = string.Format(returnedNull, name);
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
        var exceptionMessage = string.Format(threwAnException, name);
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
        var exceptionMessage = string.Format(threwAnException, name);
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

    public static Func<T1, T2, T3, Task> WrapFunc<T1, T2, T3>(this Func<T1, T2, T3, Task> func, string name)
    {
        var exceptionMessage = string.Format(threwAnException, name);
        var nullMessage = string.Format(returnedNull, name);
        return async (x, y, z) =>
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
        var exceptionMessage = string.Format(threwAnException, name);
        var nullMessage = string.Format(returnedNull, name);
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