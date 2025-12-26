using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class FileService(ILogger<FileService> logger) : IFileService
{
    public Result<byte[]> ReadFileBytes(string path)
    {
        logger.LogInformation($"{nameof(ReadFileBytes)} | Path: {{path}}", path);

        try
        {
            if (!File.Exists(path))
                return Result.Failed<byte[]>(ResultFailureType.NotFound, $"File path does not exist: \"{path}\"").Logged(logger);

            byte[] bytes = File.ReadAllBytes(path);

            return bytes.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<byte[]>().Logged(logger);
        }
    }

    public Result<string> ReadFileText(string path, Encoding? encoding = null)
    {
        logger.LogInformation($"{nameof(ReadFileText)} | Path: {{path}}", path);

        Result<byte[]> readBytesResult = ReadFileBytes(path);
        if (!readBytesResult.IsSuccessful)
            return readBytesResult.WithValue<string>().Logged(logger);

        string text = (encoding ?? Encoding.UTF8).GetString(readBytesResult.Value!);
        return text.ToOkResult().Logged(logger);
    }

    public Result WriteFileBytes(string path, byte[] data, FileAttributes? attributes = null)
    {
        logger.LogInformation($"{nameof(WriteFileBytes)} | Path: {{path}}", path);

        if (string.IsNullOrWhiteSpace(path))
            return Result.Failed(ResultFailureType.BadRequest, $"{nameof(path)} cannot be null/whitespace.");

        try
        {
            EnsureDirectoryCreated(path);
            File.WriteAllBytes(path, data);

            if (attributes.HasValue)
                File.SetAttributes(path, attributes.Value);

            return Result.Ok().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }

    public Result WriteFileText(string path, string data, FileAttributes? attributes = null, Encoding? encoding = null)
    {
        logger.LogInformation($"{nameof(WriteFileText)} | Path: {{path}}", path);

        if (string.IsNullOrWhiteSpace(path))
            return Result.Failed(ResultFailureType.BadRequest, $"{nameof(path)} cannot be null/whitespace.");

        byte[] dataBytes = (encoding ?? Encoding.UTF8).GetBytes(data);

        var writeResult = WriteFileBytes(path, dataBytes, attributes);

        return writeResult.Logged(logger);
    }

    private static void EnsureDirectoryCreated(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }
}
