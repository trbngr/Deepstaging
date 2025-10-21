// ReSharper disable InconsistentNaming
namespace Deepstaging.Data;

public static class ResultExtensions
{
    /// <summary>
    /// Converts a value to a Result with Success status.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <param name="status">An optional status to override the default of Success.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>The Result{T}</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if T is a Task type. Use AsResultTask for Task types.
    /// </exception>
    public static Result<T> AsResult<T>(this T value, ResultStatus? status = null)
    {
        if(typeof(T) == typeof(Task<>) || typeof(T) == typeof(Task))
            throw new InvalidOperationException("Use AsResultTask for Task types.");
        return Result.Success(value) with { Status = status ?? ResultStatus.Success };
    }

    public static async Task<Result<T>> AsResultTask<T>(this Task<T> task, ResultStatus? status = null,
        ResultStatus? errorStatus = null)
    {
        try
        {
            var value = await task.ConfigureAwait(false);
            return Result.Success(value) with { Status = status ?? ResultStatus.Success };
        }
        catch (Exception e)
        {
            return Result.Error<T>(e.Message) with { Status = errorStatus ?? ResultStatus.Error };
        }
    }

    public static Result<IEnumerable<T>> Where<T>(this Result<IEnumerable<T>> @this, Func<T, bool> predicate) =>
        @this switch
        {
            Result<IEnumerable<T>>.Success s => Result.Success(s.Value.Where(predicate)),
            _ => @this
        };

    public static Result<B> Select<A, B>(this Result<A> @this, Func<A, B> selector) =>
        @this switch
        {
            Result<A>.Success s => Result.Success(selector(s.Value!)),
            Result<A>.Error e => Result<B>.FromError(e),
            _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null)
        };

    public static Result<C> SelectMany<A, B, C>(this Result<A> @this,
        Func<A, Result<B>> bind,
        Func<A, B, C> project
    ) => @this switch
    {
        Result<A>.Success sa => bind(sa.Value!) switch
        {
            Result<B>.Success sb => Result.Success(project(sa.Value!, sb.Value!)),
            Result<B>.Error e => Result<C>.FromError(e),
            _ => throw new ArgumentOutOfRangeException()
        },
        Result<A>.Error er => Result<C>.FromError(er),
        _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null)
    };

    public static async Task<Result<IEnumerable<T>>> Where<T>(
        this Task<Result<IEnumerable<T>>> task,
        Func<T, bool> predicate)
    {
        var result = await task.ConfigureAwait(false);
        return result switch
        {
            Result<IEnumerable<T>>.Success s => Result.Success(s.Value.Where(predicate)),
            _ => result
        };
    }

    public static async Task<Result<B>> Select<A, B>(this Task<Result<A>> task, Func<A, B> selector)
    {
        var result = await task.ConfigureAwait(false);
        return result.Select(selector);
    }

    public static async Task<Result<C>> SelectMany<A, B, C>(
        this Task<Result<A>> task,
        Func<A, Task<Result<B>>> bind,
        Func<A, B, C> project)
    {
        var resultA = await task.ConfigureAwait(false);

        return resultA switch
        {
            Result<A>.Success sa => await bind(sa.Value!).ConfigureAwait(false) switch
            {
                Result<B>.Success sb => Result.Success(project(sa.Value!, sb.Value!)),
                Result<B>.Error e => Result<C>.FromError(e),
                _ => throw new ArgumentOutOfRangeException()
            },
            Result<A>.Error err => Result<C>.FromError(err),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}