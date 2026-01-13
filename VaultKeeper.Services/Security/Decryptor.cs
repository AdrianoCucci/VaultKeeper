using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.Services.Security;

public class Decryptor : IDecryptor
{
    private Aes? _aes;
    private ICryptoTransform? _cryptoTransform;

    private bool _disposed;

    public Decryptor(Aes aes)
    {
        _aes = aes;
        _cryptoTransform = _aes.CreateDecryptor(_aes.Key, _aes.IV);
    }

    public string DecryptToString(byte[] data, Encoding? encoding = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        using MemoryStream memoryStream = new(data);
        using CryptoStream cryptoStream = new(memoryStream, _cryptoTransform!, CryptoStreamMode.Read);
        using StreamReader reader = new(cryptoStream, encoding ?? Encoding.UTF8);

        return reader.ReadToEnd();
    }

    public string DecryptToString(string data, Encoding? encoding = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return DecryptToString(Convert.FromBase64String(data), encoding);
    }

    public byte[] DecryptToBytes(string data, Encoding? encoding = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        string decryptedString = DecryptToString(data, encoding);
        return Convert.FromBase64String(decryptedString);
    }

    public byte[] DecryptToBytes(byte[] data, Encoding? encoding = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return DecryptToBytes(Convert.ToBase64String(data), encoding);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _aes?.Dispose();
        _aes = null;

        _cryptoTransform?.Dispose();
        _cryptoTransform = null;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
