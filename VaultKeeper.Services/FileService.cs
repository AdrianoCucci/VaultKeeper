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

        Result<byte[]> result = ReadFileBytesInternal(path);

        return result.Logged(logger);
    }

    public Result<string> ReadFileText(string path, Encoding? encoding = null)
    {
        logger.LogInformation($"{nameof(ReadFileText)} | Path: {{path}}", path);

        Result<byte[]> readBytesResult = ReadFileBytesInternal(path);
        if (!readBytesResult.IsSuccessful)
            return readBytesResult.WithValue<string>().Logged(logger);

        string text = (encoding ?? Encoding.UTF8).GetString(readBytesResult.Value!);

        return text.ToOkResult().Logged(logger);
    }

    public Result WriteFileBytes(string path, byte[] data, FileAttributes? attributes = null)
    {
        logger.LogInformation($"{nameof(WriteFileBytes)} | Path: {{path}}", path);

        Result result = WriteFileBytesInternal(path, data, attributes);

        return result.Logged(logger);
    }

    public Result WriteFileText(string path, string data, FileAttributes? attributes = null, Encoding? encoding = null)
    {
        logger.LogInformation($"{nameof(WriteFileText)} | Path: {{path}}", path);

        Result result = WriteFileTextInternal(path, data, attributes);

        return result.Logged(logger);
    }

    public Result DeleteFileAsync(string path)
    {
        Result result = RunCatching(path, () => File.Delete(path));
        if (result.FailureType == ResultFailureType.NotFound)
        {
            logger.LogInformation("Requested file to delete does not exist: \"{path}\"", path);
            result = Result.Ok();
        }

        return result.Logged(logger);
    }

    public bool FileExists(string? path)
    {
        logger.LogInformation($"{nameof(FileExists)} | path: {{path}}", path);
        return File.Exists(path);
    }

    public Result CanReadFile(string path)
    {
        logger.LogInformation($"{nameof(CanReadFile)} | path: {{path}}", path);

        Result result = RunCatching(path, () => File.OpenRead(path).Dispose());
        if (result.IsSuccessful)
            result = result with { Message = $"File path can be read: \"{path}\"." };

        return result.Logged(logger);
    }

    public Result CanWriteToDirectory(string path)
    {
        logger.LogInformation($"{nameof(CanWriteToDirectory)} | path: {{path}}", path);

        Result result = RunCatching(path, () =>
        {
            string tempFilePath = Path.Combine(path, Path.GetRandomFileName());
            File.Create(tempFilePath, 1, FileOptions.DeleteOnClose).Dispose();
        });

        return result.Logged(logger);
    }

    private static Result<byte[]> ReadFileBytesInternal(string path) => RunCatching(path, () => File.ReadAllBytes(path));

    private static Result WriteFileBytesInternal(string path, byte[] data, FileAttributes? attributes = null) => RunCatching(path, () =>
    {
        EnsureDirectoryCreated(path);
        File.WriteAllBytes(path, data);

        if (attributes.HasValue)
            File.SetAttributes(path, attributes.Value);
    });

    private static Result WriteFileTextInternal(string path, string data, FileAttributes? attributes = null, Encoding? encoding = null)
    {
        byte[] dataBytes = (encoding ?? Encoding.UTF8).GetBytes(data);
        return WriteFileBytesInternal(path, dataBytes, attributes);
    }

    private static void EnsureDirectoryCreated(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }

    private static Result<T> RunCatching<T>(string path, Func<T> handler)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Result.Failed<T>(ResultFailureType.BadRequest, $"{nameof(path)} cannot be null/whitespace.");

        try
        {
            T returnValue = handler.Invoke();
            return returnValue.ToOkResult();
        }
        catch (FileNotFoundException ex)
        {
            return Result.Failed<T>(ResultFailureType.NotFound, $"File does not exist: \"{path}\"", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.ToFailedResult<T>(ResultFailureType.Unauthorized);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<T>();
        }
    }

    private static Result RunCatching(string path, Action handler) => RunCatching<object>(path, () =>
    {
        handler.Invoke();
        return null!;
    }).WithoutValue();
}
