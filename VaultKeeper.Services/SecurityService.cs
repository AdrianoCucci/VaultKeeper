using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class SecurityService(ILogger<SecurityService> logger) : ISecurityService
{
    private const string _key = "KlGIFd47WBntmehEce+iY+2KTxWLOlh0TLf2RaQwIQs=";
    private const string _iv = "WgsQXhiD/N60B4z2Ltppfg==";

    public Result<string> Encrypt(string value)
    {
        logger.LogInformation(nameof(Encrypt));

        try
        {
            using Aes aes = CreateAes();

            using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using StreamWriter writer = new(cryptoStream);
            writer.Write(value);
            writer.Close();

            byte[] encryptedValue = memoryStream.ToArray();

            return Convert.ToBase64String(encryptedValue).ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string>().Logged(logger);
        }
    }

    public Result<string> Decrypt(string value)
    {
        logger.LogInformation(nameof(Decrypt));

        try
        {
            using Aes aes = CreateAes();

            using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using MemoryStream memoryStream = new(Convert.FromBase64String(value));
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader reader = new(cryptoStream);

            string decryptedValue = reader.ReadToEnd();

            return decryptedValue.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string>().Logged(logger);
        }
    }

    public Result<string> CreateHash(string value, Encoding? encoding = null)
    {
        logger.LogInformation(nameof(CreateHash));

        try
        {
            encoding ??= Encoding.UTF8;
            byte[] valueBytes = encoding.GetBytes(value);
            byte[] hashBytes = SHA256.HashData(valueBytes);
            string hashText = Convert.ToBase64String(hashBytes);

            return hashText.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<string>().Logged(logger);
        }
    }

    public Result<bool> CompareHash(string value, string hash, Encoding? encoding = null)
    {
        logger.LogInformation(nameof(CompareHash));

        var createHashResult = CreateHash(value, encoding);
        if (!createHashResult.IsSuccessful)
            return createHashResult.WithValue<bool>();

        bool isMatch = createHashResult.Value == hash;

        return isMatch.ToOkResult().Logged(logger);
    }

    private static Aes CreateAes()
    {
        Aes aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Key = Convert.FromBase64String(_key);
        aes.IV = Convert.FromBase64String(_iv);

        return aes;
    }
}
