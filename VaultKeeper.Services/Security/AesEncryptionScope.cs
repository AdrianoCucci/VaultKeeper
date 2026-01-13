using System;
using System.Security.Cryptography;
using VaultKeeper.Models.Security;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.Services.Security;

public class AesEncryptionScope(
    AesGcm aes,
    Func<AesGcm, string, EncryptedData> encryptHandler,
    Func<AesGcm, EncryptedData, string> decryptHandler) : IEncryptionScope
{
    private readonly Func<AesGcm, string, EncryptedData> _encryptHandler = encryptHandler;
    private readonly Func<AesGcm, EncryptedData, string> _decryptHandler = decryptHandler;

    public EncryptedData Encrypt(string data) => _encryptHandler.Invoke(aes, data);

    public string Decrypt(EncryptedData encryptedData) => _decryptHandler.Invoke(aes, encryptedData);

    public string Decrypt(string encryptedText) => Decrypt(EncryptedData.Parse(encryptedText));
}
