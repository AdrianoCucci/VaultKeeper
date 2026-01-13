using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.Services.Security;

public class Encryptor : IEncryptor
{
    private Aes? _aes;
    private ICryptoTransform? _cryptoTransform;

    private bool _disposed;

    public Encryptor(Aes aes)
    {
        _aes = aes;
        _cryptoTransform = _aes.CreateEncryptor(_aes.Key, _aes.IV);
    }

    public byte[] EncryptToBytes(byte[] data, Encoding? encoding = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, _cryptoTransform!, CryptoStreamMode.Write);
        using StreamWriter writer = new(cryptoStream, encoding ?? Encoding.UTF8);
        writer.Write(data);
        writer.Close();

        return memoryStream.ToArray();
    }

    public byte[] EncryptToBytes(string data, Encoding? encoding = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        encoding ??= Encoding.UTF8;
        byte[] dataBytes = encoding.GetBytes(data);

        return EncryptToBytes(dataBytes, encoding);
    }

    public string EncryptToString(byte[] data, Encoding? encoding = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        byte[] encryptedBytes = EncryptToBytes(data, encoding);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string EncryptToString(string data, Encoding? encoding = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        byte[] encryptedBytes = EncryptToBytes(data, encoding);
        return Convert.ToBase64String(encryptedBytes);
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
