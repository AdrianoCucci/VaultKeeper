using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.Services.Security;

public class HashService(ILogger<HashService> logger) : IHashService
{
    private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
    private const int _hashSize = 32;
    private const int _saltSize = 32;
    private const int _iterations = 100000;

    public Result<string> CreateHash(string value)
    {
        logger.LogInformation(nameof(CreateHash));

        try
        {
            byte[] salt = new byte[_saltSize];
            RandomNumberGenerator.Fill(salt);

            using Rfc2898DeriveBytes pbkdf2 = new(value, salt, _iterations, _hashAlgorithmName);
            byte[] hash = pbkdf2.GetBytes(_hashSize);

            byte[] packedBytes = PackBytes(hash, salt);
            string result = Convert.ToBase64String(packedBytes);

            return result.ToOkResult().Logged(logger);
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

    public Result<bool> CompareHash(string value, string hash)
    {
        logger.LogInformation(nameof(CompareHash));

        try
        {
            byte[] packedBytes = Convert.FromBase64String(hash);
            (byte[] salt, byte[] unpackedHash) = UnpackBytes(packedBytes);

            using Rfc2898DeriveBytes pbkdf2 = new(value, salt, _iterations, _hashAlgorithmName);
            byte[] valueHash = pbkdf2.GetBytes(_hashSize);

            bool isMatch = CryptographicOperations.FixedTimeEquals(unpackedHash, valueHash);

            return isMatch.ToOkResult().Logged(logger);
        }
        catch (FormatException ex)
        {
            return ex.ToFailedResult<bool>(ResultFailureType.InvalidFormat).Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<bool>().Logged(logger);
        }
    }

    private static byte[] PackBytes(byte[] hash, byte[] salt)
    {
        // Format: [Salt (32 bytes)][Hash (32 bytes)]
        byte[] packedBytes = new byte[hash.Length + salt.Length];
        Span<byte> packedSpan = packedBytes;

        salt.CopyTo(packedSpan[.._saltSize]);
        hash.CopyTo(packedSpan[_saltSize..]);

        return packedSpan.ToArray();
    }

    private static (byte[] Salt, byte[] Hash) UnpackBytes(byte[] packedBytes)
    {
        // Format: [Salt (32 bytes)][Hash (32 bytes)]
        Span<byte> packedSpan = packedBytes;
        Span<byte> salt = packedSpan[.._saltSize];
        Span<byte> hash = packedSpan[_saltSize..];

        return (salt.ToArray(), hash.ToArray());
    }
}
