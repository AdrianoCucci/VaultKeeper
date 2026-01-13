using System;
using System.Linq;

namespace VaultKeeper.Models.Security;

public readonly record struct EncryptedData
{
    public const char Separator = '.';

    public static EncryptedData Parse(string value)
    {
        string[] parts = value.Split(Separator);

        if (parts.Length != 3 || parts.Any(string.IsNullOrWhiteSpace))
            throw new FormatException($"{nameof(value)} is not a correct {nameof(EncryptedData)} formatted string.");

        return new(value, new()
        {
            Ciphertext = parts[0],
            Nonce = parts[1],
            Tag = parts[2]
        });
    }

    public string Value { get; }
    public EncryptedDataParts Parts { get; }

    public EncryptedData(string ciphertext, string nonce, string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ciphertext, nameof(ciphertext));
        ArgumentException.ThrowIfNullOrWhiteSpace(nonce, nameof(nonce));
        ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tag));

        Value = string.Join(Separator, ciphertext, nonce, tag);
        Parts = new()
        {
            Ciphertext = ciphertext,
            Nonce = nonce,
            Tag = tag
        };
    }

    public EncryptedData() => Value = string.Empty;

    private EncryptedData(string value, EncryptedDataParts parts)
    {
        Value = value;
        Parts = parts;
    }

    public static implicit operator string(EncryptedData encryptedData) => encryptedData.Value;

    public readonly record struct EncryptedDataParts
    {
        public required string Ciphertext { get; init; }
        public required string Nonce { get; init; }
        public required string Tag { get; init; }

        public EncryptedDataParts()
        {
            Ciphertext = string.Empty;
            Nonce = string.Empty;
            Tag = string.Empty;
        }
    }
}
