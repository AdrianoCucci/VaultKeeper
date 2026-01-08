using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public static Result ToAggregatedResult(this IEnumerable<Result> results, ResultFailureType defaultFailureType = ResultFailureType.Unknown)
    {
        Result[] failedResults = [.. results.Where(x => !x.IsSuccessful)];

        if (failedResults.Length < 1)
            return Result.Ok();

        IEnumerable<string> errorMessages = failedResults.Select(x => $"- ({x.FailureType}): {x.Message ?? x.Exception?.Message ?? "Unknown error."}".Trim());

        ResultFailureType failureType = failedResults
            .Select(x => x.FailureType)
            .Distinct()
            .Count() < 2
                ? failedResults.First().FailureType ?? defaultFailureType
                : defaultFailureType;

        AggregateException exception = new(failedResults.Where(x => x.Exception != null).Select(x => x.Exception!));

        return Result.Failed(failureType, $"({failedResults.Length}/{results.Count()}) results failed:\n{string.Join('\n', errorMessages)}", exception);
    }

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

    public static Result<T> Logged<T>(this Result<T> result, ILogger logger, bool failedAsWarning = false)
    {
        if (result.IsSuccessful)
        {
            logger.LogInformation("Result OK | Value: {value} | Message: {message}", result.Value, result.Message);
        }
        else
        {
            const string template = "Result FAILED | Status: {status} | Value: {value} | Message: {message}";

            if (failedAsWarning)
                logger.LogWarning(result.Exception, template, result.FailureType, result.Value, result.Message);
            else
                logger.LogError(result.Exception, template, result.FailureType, result.Value, result.Message);
        }

        return result;
    }
}
