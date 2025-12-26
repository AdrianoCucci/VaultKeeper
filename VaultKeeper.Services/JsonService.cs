using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class JsonService(ILogger<JsonService> logger) : IJsonService
{
    private static readonly JsonSerializerOptions _jsonOptions = new();

    public Result<string> Serialize<T>(T data)
    {
        logger.LogInformation(nameof(Serialize));

        try
        {
            string json = JsonSerializer.Serialize(data, _jsonOptions);
            return json.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string>().Logged(logger);
        }
    }

    public Result<T> Deserialize<T>(string json)
    {
        logger.LogInformation(nameof(Deserialize));

        try
        {
            T data = JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
            return data.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<T>().Logged(logger);
        }
    }
}
