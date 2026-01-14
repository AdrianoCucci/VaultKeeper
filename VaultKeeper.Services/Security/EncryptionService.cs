using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Security;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.DataFormatting;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.Services.Security;

public class EncryptionService(ILogger<EncryptionService> logger, IFileService fileService, IJsonService jsonService) : IEncryptionService
{
    private static int NonceSizeBytes => AesGcm.NonceByteSizes.MaxSize;
    private static int TagSizeBytes => AesGcm.TagByteSizes.MaxSize;

    private static EncryptionConfig DefaultEncryptionConfig => new()
    {
        Key = "KlGIFd47WBntmehEce+iY+2KTxWLOlh0TLf2RaQwIQs=",
        IV = "WgsQXhiD/N60B4z2Ltppfg=="
    };

    private string? _encryptionConfigFilePath;

    public Result SaveEncryptionConfigToFile(string filePath, EncryptionConfig config)
    {
        logger.LogInformation(nameof(SaveEncryptionConfigToFile));

        try
        {
            Result<string> serializeResult = jsonService.Serialize(config);
            if (!serializeResult.IsSuccessful)
                return serializeResult.Logged(logger);

            Result writeFileResult = fileService.WriteFileText(filePath, serializeResult.Value!);
            return writeFileResult.Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }

    public void UseEncryptionConfigFromFile(string? filePath)
    {
        logger.LogInformation(nameof(UseEncryptionConfigFromFile));
        _encryptionConfigFilePath = filePath;
    }

    public Result<EncryptedData> Encrypt(string data)
    {
        logger.LogInformation(nameof(Encrypt));

        try
        {
            Result<EncryptionConfig> getConfigResult = GetEncryptionConfigOrDefault();
            if (!getConfigResult.IsSuccessful)
                return getConfigResult.WithValue<EncryptedData>().Logged(logger);

            byte[] key = Convert.FromBase64String(getConfigResult.Value!.Key);
            EncryptedData encryptedData = EncryptInternal(data, key);

            return encryptedData.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<EncryptedData>().Logged(logger);
        }
    }

    public Result<string> Decrypt(EncryptedData data)
    {
        logger.LogInformation($"{nameof(Decrypt)} ({{paramType}})", data.GetType().Name);

        try
        {
            Result<EncryptionConfig> getConfigResult = GetEncryptionConfigOrDefault();
            if (!getConfigResult.IsSuccessful)
                return getConfigResult.WithValue<string>().Logged(logger);

            byte[] key = Convert.FromBase64String(getConfigResult.Value!.Key);
            string decryptedData = DecryptInternal(data, key);

            return decryptedData.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string>().Logged(logger);
        }
    }

    public Result<string> Decrypt(string data)
    {
        logger.LogInformation($"{nameof(Decrypt)} ({{paramType}})", data.GetType().Name);

        try
        {
            EncryptedData encryptedData = EncryptedData.Parse(data);

            Result<EncryptionConfig> getConfigResult = GetEncryptionConfigOrDefault();
            if (!getConfigResult.IsSuccessful)
                return getConfigResult.WithValue<string>().Logged(logger);

            byte[] key = Convert.FromBase64String(getConfigResult.Value!.Key);
            string decryptedData = DecryptInternal(encryptedData, key);

            return decryptedData.ToOkResult().Logged(logger);
        }
        catch (FormatException ex)
        {
            return ex.ToFailedResult<string>(ResultFailureType.InvalidFormat).Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string>().Logged(logger);
        }
    }

    public Result UsingEncryptionScope(Action<IEncryptionScope> scopeFunc)
    {
        logger.LogInformation(nameof(UsingEncryptionScope));

        try
        {
            Result<EncryptionConfig> getConfigResult = GetEncryptionConfigOrDefault();
            if (!getConfigResult.IsSuccessful)
                return getConfigResult.WithValue<EncryptedData>().Logged(logger);

            byte[] key = Convert.FromBase64String(getConfigResult.Value!.Key);

            using AesGcm aes = CreateAes(key);
            AesEncryptionScope scope = new(aes, EncryptInternal, DecryptInternal);
            scopeFunc.Invoke(scope);

            return Result.Ok().Logged(logger);
        }
        catch (FormatException ex)
        {
            return ex.ToFailedResult<string>(ResultFailureType.InvalidFormat).Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }

    private static EncryptedData EncryptInternal(AesGcm aes, string data)
    {
        byte[] nonce = new byte[NonceSizeBytes];
        RandomNumberGenerator.Fill(nonce);

        byte[] plaintext = Encoding.UTF8.GetBytes(data);
        byte[] cyphertext = new byte[plaintext.Length];
        byte[] tag = new byte[TagSizeBytes];

        aes.Encrypt(nonce, plaintext, cyphertext, tag);

        string cyphertextString = Convert.ToBase64String(cyphertext);
        string nonceString = Convert.ToBase64String(nonce);
        string tagString = Convert.ToBase64String(tag);

        return new EncryptedData(cyphertextString, nonceString, tagString);
    }

    private static EncryptedData EncryptInternal(string data, byte[] key)
    {
        using AesGcm aes = CreateAes(key);
        return EncryptInternal(aes, data);
    }

    private static string DecryptInternal(AesGcm aes, EncryptedData data)
    {
        EncryptedData.EncryptedDataParts parts = data.Parts;

        byte[] ciphertext = Convert.FromBase64String(parts.Ciphertext);
        byte[] nonce = Convert.FromBase64String(parts.Nonce);
        byte[] tag = Convert.FromBase64String(parts.Tag);
        byte[] plaintext = new byte[ciphertext.Length];

        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    private static string DecryptInternal(EncryptedData data, byte[] key)
    {
        using AesGcm aes = CreateAes(key);
        return DecryptInternal(aes, data);
    }

    private static AesGcm CreateAes(byte[] key) => new(key, TagSizeBytes);

    private Result<EncryptionConfig> GetEncryptionConfigOrDefault()
    {
        if (string.IsNullOrWhiteSpace(_encryptionConfigFilePath))
            return DefaultEncryptionConfig.ToOkResult();

        return LoadEncryptionConfigFromFile(_encryptionConfigFilePath);
    }

    private Result<EncryptionConfig> LoadEncryptionConfigFromFile(string filePath)
    {
        static string FormatFailMessage(Result failResult) => $"Failed to read encryption config file path ({failResult.FailureType}): {failResult.Message}";

        Result<string> readFileResult = fileService.ReadFileText(filePath);
        if (!readFileResult.IsSuccessful)
            return readFileResult.WithValue<EncryptionConfig>() with { Message = FormatFailMessage(readFileResult) };

        Result<EncryptionConfig> deserializeResult = jsonService.Deserialize<EncryptionConfig>(readFileResult.Value!);
        if (!deserializeResult.IsSuccessful)
            return deserializeResult.WithValue<EncryptionConfig>() with { Message = FormatFailMessage(readFileResult) };

        return deserializeResult.Value!.ToOkResult();
    }
}