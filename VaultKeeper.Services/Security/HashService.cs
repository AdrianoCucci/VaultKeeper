using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.Services.Security;

public class HashService(ILogger<HashService> logger) : IHashService
{
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
}
