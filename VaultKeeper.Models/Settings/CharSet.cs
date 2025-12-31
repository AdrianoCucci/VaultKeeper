using System.Collections.Generic;

namespace VaultKeeper.Models.Settings;

public record CharSet
{
    public required CharSetType Type { get; set; }
    public required IEnumerable<char> Chars { get; set; }
}