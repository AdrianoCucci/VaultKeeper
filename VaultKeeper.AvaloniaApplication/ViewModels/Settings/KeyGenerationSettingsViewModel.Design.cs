using System;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Settings;

public partial class KeyGenerationSettingsViewModel
{
    public static KeyGenerationSettingsViewModel Design => _designLazy.Value;

    private static readonly Lazy<KeyGenerationSettingsViewModel> _designLazy = new(() =>
    {
        CharSet[] charSetOptions =
        [
            CharSet.Default,
            new() { Type = CharSetType.LettersOnly, Name = "CharSet Option 2", Chars = "abc" },
            new() { Type = CharSetType.NumbersOnly, Name = "CharSet Option 3", Chars = "123" },
            new() { Type = CharSetType.NumbersAndSymbols, Name = "CharSet Option 4", Chars = "123!@#" },
            new() { Type = CharSetType.SymbolsOnly, Name = "CharSet Option 5", Chars = "!@#" },
        ];

        return new()
        {
            CharSetOptions = charSetOptions,
            CharSet = charSetOptions[1]
        };
    });
}
