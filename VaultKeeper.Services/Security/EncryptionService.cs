using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.Services.Security;

public class EncryptionService(ILogger<EncryptionService> logger, IFileService fileService) : IEncryptionService
{
    private static readonly int _keySizeBytes = 32;
    private static readonly int _nonceSizeBytes = AesGcm.NonceByteSizes.MaxSize;
    private static readonly int _tagSizeBytes = AesGcm.TagByteSizes.MaxSize;
    private const string _keyFileSeparator = "----------";

    // System default key if user has not set a custom key to use. Treat as insecure.
    private static string DefaultEncryptionKey => Convert.ToBase64String(
    [
        42, 81, 136, 21, 222, 59, 88, 25,
        237, 153, 232, 68, 113, 239, 162, 99,
        237, 138, 79, 21, 139, 58, 88, 116,
        76, 183, 246, 69, 164, 48, 33, 11
    ]);

    private string? _encryptionConfigFilePath;

    public Result<string> GenerateEncryptionKey()
    {
        logger.LogInformation(nameof(GenerateEncryptionKey));

        try
        {
            string key = GenerateEncryptionKeyInternal();

            Result<string> result = key.ToOkResult();
            _ = result.WithoutValue().Logged(logger); // Do not log encryption key.

            return result;
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string>().Logged(logger);
        }
    }

    public Result<string> GenerateEncryptionKeyFileData(string? key = null)
    {
        logger.LogInformation(nameof(GenerateEncryptionKeyFileData));

        try
        {
            if (string.IsNullOrWhiteSpace(key))
                key = GenerateEncryptionKeyInternal();

            string fileData = CreateKeyFileData(key);

            Result<string> result = fileData.ToOkResult();
            _ = result.WithoutValue().Logged(logger); // Do not log encryption key.

            return result;
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string>().Logged(logger);
        }
    }

    public Result VerifyValidEncryptionKeyFile(string? filePath)
    {
        logger.LogInformation(nameof(VerifyValidEncryptionKeyFile));

        Result canReadResult = ReadEncryptionKeyFromFile(filePath).WithoutValue();

        return canReadResult.Logged(logger);
    }

    public void UseSystemEncryptionKey() => _encryptionConfigFilePath = null;

    public bool IsUsingDefaultEncryptionKey() => _encryptionConfigFilePath == null;

    public Result UseEncryptionKeyFile(string filePath)
    {
        logger.LogInformation(nameof(UseEncryptionKeyFile));

        Result readKeyFromFileResult = ReadEncryptionKeyFromFile(filePath).WithoutValue();
        if (!readKeyFromFileResult.IsSuccessful)
            return readKeyFromFileResult.Logged(logger);

        _encryptionConfigFilePath = filePath;

        return Result.Ok("Encryption key file path is valid and key can be parsed - using file.").Logged(logger);
    }

    public Result<string> Encrypt(string data, bool forceUseSystemKey = false)
    {
        logger.LogInformation(nameof(Encrypt));

        try
        {
            Result<string> getKeyResult = GetEncryptionKeyOrDefault(forceUseSystemKey);
            if (!getKeyResult.IsSuccessful)
                return getKeyResult.WithValue<string>().Logged(logger);

            byte[] key = Convert.FromBase64String(getKeyResult.Value!);
            string encryptedData = EncryptInternal(data, key);

            return encryptedData.ToOkResult().Logged(logger);
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

    public Result<string> Decrypt(string data, bool forceUseSystemKey = false)
    {
        logger.LogInformation(nameof(Decrypt));

        try
        {
            Result<string> getKeyResult = GetEncryptionKeyOrDefault(forceUseSystemKey);
            if (!getKeyResult.IsSuccessful)
                return getKeyResult.WithValue<string>().Logged(logger);

            byte[] key = Convert.FromBase64String(getKeyResult.Value!);
            string decryptedData = DecryptInternal(data, key);

            return decryptedData.ToOkResult().Logged(logger);
        }
        catch (FormatException ex)
        {
            return ex.ToFailedResult<string>(ResultFailureType.InvalidFormat).Logged(logger);
        }
        catch (AuthenticationTagMismatchException ex)
        {
            return ex.ToFailedResult<string>(ResultFailureType.Unauthorized).Logged(logger);
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
            Result<string> getKeyResult = GetEncryptionKeyOrDefault();
            if (!getKeyResult.IsSuccessful)
                return getKeyResult.Logged(logger);

            byte[] key = Convert.FromBase64String(getKeyResult.Value!);

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

    private static AesGcm CreateAes(byte[] key) => new(key, _tagSizeBytes);

    private static string GenerateEncryptionKeyInternal()
    {
        byte[] key = new byte[_keySizeBytes];
        RandomNumberGenerator.Fill(key);

        return Convert.ToBase64String(key);
    }

    private static string EncryptInternal(AesGcm aes, string data)
    {
        byte[] nonce = new byte[_nonceSizeBytes];
        RandomNumberGenerator.Fill(nonce);

        byte[] plaintext = Encoding.UTF8.GetBytes(data);
        byte[] cyphertext = new byte[plaintext.Length];
        byte[] tag = new byte[_tagSizeBytes];

        aes.Encrypt(nonce, plaintext, cyphertext, tag);

        byte[] packedBytes = PackBytes(nonce, tag, cyphertext);
        string encryptedString = Convert.ToBase64String(packedBytes);

        return encryptedString;
    }

    private static string EncryptInternal(string data, byte[] key)
    {
        using AesGcm aes = CreateAes(key);
        return EncryptInternal(aes, data);
    }

    private static string DecryptInternal(AesGcm aes, string data)
    {
        try
        {
            byte[] packedBytes = Convert.FromBase64String(data);
            (byte[] nonce, byte[] tag, byte[] ciphertext) = UnpackBytes(packedBytes);
            byte[] plaintext = new byte[ciphertext.Length];

            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
        catch (AuthenticationTagMismatchException ex)
        {
            throw new AuthenticationTagMismatchException("Data decryption failed - the encryption key is invalid.", ex);
        }
        catch (Exception ex)
        {
            throw new FormatException("Data decryption failed - data may be corrupted or malformed.", ex);
        }
    }

    private static string DecryptInternal(string data, byte[] key)
    {
        using AesGcm aes = CreateAes(key);
        return DecryptInternal(aes, data);
    }

    private static byte[] PackBytes(byte[] nonce, byte[] tag, byte[] ciphertext)
    {
        // Format: [Nonce (12 bytes)][Tag (16 bytes)][Ciphertext (variable)]
        byte[] packedBytes = new byte[_nonceSizeBytes + _tagSizeBytes + ciphertext.Length];
        Span<byte> packedSpan = packedBytes;

        nonce.CopyTo(packedSpan[.._nonceSizeBytes]);
        tag.CopyTo(packedSpan.Slice(_nonceSizeBytes, _tagSizeBytes));
        ciphertext.CopyTo(packedSpan[(_nonceSizeBytes + _tagSizeBytes)..]);

        return packedSpan.ToArray();
    }

    private static (byte[] Nonce, byte[] Tag, byte[] Ciphertext) UnpackBytes(byte[] packedBytes)
    {
        // Format: [Nonce (12 bytes)][Tag (16 bytes)][Ciphertext (variable)]
        Span<byte> packedSpan = packedBytes;
        Span<byte> nonce = packedSpan[.._nonceSizeBytes];
        Span<byte> tag = packedSpan.Slice(_nonceSizeBytes, _tagSizeBytes);
        Span<byte> ciphertext = packedSpan[(_nonceSizeBytes + _tagSizeBytes)..];

        return (nonce.ToArray(), tag.ToArray(), ciphertext.ToArray());
    }

    private static string CreateKeyFileData(string key)
    {
        StringBuilder sb = new();
        sb.AppendJoin(Environment.NewLine,
        [
            "This is your Vault Keeper encryption key - it is used to encrypt and decryped your saved keys.",
            "Do not modify or share this key with anyone.",
            string.Empty,
            "IMPORTANT: Make sure to backup this file - if your key is lost, corrupted, or modified,",
            "then your saved Vault Keeper data will be impossible to access.",
            string.Empty,
            "<BEGIN-KEY>",
            _keyFileSeparator,
            key,
            _keyFileSeparator,
            "<END-KEY>"
        ]);

        return sb.ToString();
    }

    private static Result<string?> ParseKeyFromKeyFile(string keyFileData)
    {
        try
        {
            string formattedData = keyFileData.Replace(Environment.NewLine, string.Empty);
            int startSeparatorIndex = formattedData.IndexOf(_keyFileSeparator) + _keyFileSeparator.Length;
            int endSeparatorIndex = formattedData.LastIndexOf(_keyFileSeparator);
            string? key = formattedData[startSeparatorIndex..endSeparatorIndex];

            if (string.IsNullOrWhiteSpace(key))
                return Result.Failed<string?>(ResultFailureType.InvalidFormat, "Encryption key could not be found in file - file may be corrupted or malformed.");

            byte[] keyBytes = Convert.FromBase64String(key);
            bool isValidSize = keyBytes.Length == _keySizeBytes;
            if (!isValidSize)
                return Result.Failed<string?>(ResultFailureType.InvalidFormat, $"Encryption key found in file is not a valid byte length - file may be corrupted or malformed.");

            return Result.Ok<string?>(key);
        }
        catch (FormatException ex)
        {
            return ex.ToFailedResult<string?>(ResultFailureType.InvalidFormat, "Encryption key found in file is not a valid Base64-encoded format - file may be corrupted or malformed.");
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string?>();
        }
    }

    private Result<string> GetEncryptionKeyOrDefault(bool forceUseSystemKey = false)
    {
        if (string.IsNullOrWhiteSpace(_encryptionConfigFilePath) || forceUseSystemKey)
            return DefaultEncryptionKey.ToOkResult();

        return ReadEncryptionKeyFromFile(_encryptionConfigFilePath);
    }

    private Result<string> ReadEncryptionKeyFromFile(string? filePath)
    {
        const string failMessagePrefix = "Failed to read encryption config file path";

        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Result.Failed<string>(ResultFailureType.BadRequest, $"{failMessagePrefix}: provided file path is empty.");

            Result<string> readFileResult = fileService.ReadFileText(filePath);
            if (!readFileResult.IsSuccessful)
                return readFileResult.WithValue<string>() with { Message = $"{failMessagePrefix}: ({readFileResult.FailureType}): {readFileResult.Message}" };

            Result<string?> parseKeyResult = ParseKeyFromKeyFile(readFileResult.Value!);
            if (!parseKeyResult.IsSuccessful)
                return parseKeyResult.WithValue<string>();

            return parseKeyResult!;
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string>() with { Message = $"{failMessagePrefix}: {ex.Message}" };
        }
    }
}