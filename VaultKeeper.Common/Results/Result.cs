using System;

namespace VaultKeeper.Common.Results;

public record Result
{
    public static Result Ok(string? message = null) => new()
    {
        Message = message
    };

    public static Result<T> Ok<T>(T? value, string? message = null) => new()
    {
        Value = value,
        Message = message
    };

    public static Result Failed(ResultFailureType failureType, string? message = null, Exception? exception = null) => new()
    {
        FailureType = failureType,
        Message = message,
        Exception = exception
    };

    public static Result<T> Failed<T>(ResultFailureType failureType, string? message = null, Exception? exception = null) => new()
    {
        FailureType = failureType,
        Message = message,
        Exception = exception
    };

    public static Result Failed(string? message = null, Exception? exception = null) => Failed(ResultFailureType.Unknown, message, exception);

    public static Result<T> Failed<T>(string? message = null, Exception? exception = null) => Failed<T>(ResultFailureType.Unknown, message, exception);

    public ResultFailureType? FailureType { get; init; }
    public string? Message { get; init; }
    public Exception? Exception { get; init; }

    public bool IsSuccessful => !FailureType.HasValue;
}

public record Result<T> : Result
{
    public T? Value { get; init; }
}

public enum ResultFailureType
{
    Unknown,
    BadRequest,
    NotFound,
    Conflict,
    InvalidFormat
}