using Microsoft.Extensions.Logging;
using System;
using VaultKeeper.Common.Results;

namespace VaultKeeper.Common.Extensions;

public static class ResultExtensions
{
    public static Result WithoutValue<T>(this Result<T> result) => new()
    {
        FailureType = result.FailureType,
        Message = result.Message,
        Exception = result.Exception
    };

    public static Result<T> WithValue<T>(this Result result, T? value = default) => new()
    {
        Value = value,
        FailureType = result.FailureType,
        Message = result.Message,
        Exception = result.Exception
    };

    public static Result<T> ToOkResult<T>(this T value) => Result.Ok(value);

    public static Result ToFailedResult(this Exception exception, ResultFailureType failureType = ResultFailureType.Unknown, string? message = null) =>
        Result.Failed(failureType, message ?? exception.Message ?? "Unknown error", exception);

    public static Result<T> ToFailedResult<T>(this Exception exception, ResultFailureType failureType = ResultFailureType.Unknown, string? message = null) =>
        Result.Failed<T>(failureType, message ?? exception.Message ?? "Unknown error", exception);

    public static TResult Logged<TResult>(this TResult result, ILogger logger, bool failedAsWarning = false) where TResult : Result
    {
        if (result.IsSuccessful)
        {
            logger.LogInformation("Result OK | Message: {message}", result.Message);
        }
        else
        {
            const string template = "Result FAILED | Status: {status} | Message: {message}";

            if (failedAsWarning)
                logger.LogWarning(result.Exception, template, result.FailureType, result.Message);
            else
                logger.LogError(result.Exception, template, result.FailureType, result.Message);
        }

        return result;
    }
}
