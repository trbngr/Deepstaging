// ReSharper disable InconsistentNaming

// ReSharper disable NotAccessedPositionalProperty.Global
namespace Deepstaging.Data;

public static class Result
{
    public static Result<T> Success<T>(T value) =>
        new Result<T>.Success(value) { Status = ResultStatus.Success };

    public static Result<T> Error<T>(string message, Exception? exception = null, ResultStatus? status = null) =>
        new Result<T>.Error(message, exception) { Status = status ?? ResultStatus.Error };
}

[Dunet.Union]
public partial record Result<T>
{
    public required ResultStatus Status { get; init; }
    
    partial record Error(string Message, Exception? Exception);

    partial record Success(T Value);
    public static implicit operator Result<T>(T value) => new Success(value) { Status = ResultStatus.Success };

    public static implicit operator T(Result<T> result) => result switch
    {
        Success success => success.Value,
        Error error => throw new InvalidOperationException(
            $"Cannot convert Error result to value.\n\t{error.Status}:\n\t\t{error.Message}"),
        _ => throw new InvalidOperationException("Unknown result type")
    };

    public static Result<T> FromError<A>(Result<A>.Error error) => 
        Result.Error<T>(error.Message)with { Status = error.Status };
}