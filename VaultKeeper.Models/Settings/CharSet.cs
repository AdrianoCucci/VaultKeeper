using System.Collections.Generic;

namespace VaultKeeper.Models.Settings;

public record CharSet
{
    public static CharSet Default => new()
    {
        Type = CharSetType.AlphaNumericAndSymbols,
        Name = "Alpha-Numeric + Symbols",
        Chars = "abcdefghijklmnopkrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?"
    };

    public required CharSetType Type { get; set; }
    public required string Name { get; set; }
    public required IEnumerable<char> Chars { get; set; }
}