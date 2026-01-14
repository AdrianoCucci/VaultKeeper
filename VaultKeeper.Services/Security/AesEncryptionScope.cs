using System;
using System.Security.Cryptography;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.Services.Security;

public class AesEncryptionScope(
    AesGcm aes,
    Func<AesGcm, string, string> encryptHandler,
    Func<AesGcm, string, string> decryptHandler) : IEncryptionScope
{
    private readonly Func<AesGcm, string, string> _encryptHandler = encryptHandler;
    private readonly Func<AesGcm, string, string> _decryptHandler = decryptHandler;

    public string Encrypt(string data) => _encryptHandler.Invoke(aes, data);

    public string Decrypt(string encryptedData) => _decryptHandler.Invoke(aes, encryptedData);
}
