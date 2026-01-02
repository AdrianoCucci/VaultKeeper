using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class CharSetService(ILogger<CharSetService> logger) : ICharSetService
{
    private const string _lettersCharSet = "abcdefghijklmnopkrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string _numbersCharSet = "0123456789";
    private const string _symbolsCharSet = "`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?";

    private static HashSet<CharSet> CharSets =>
    [
        new()
        {
            Type = CharSetType.AlphaNumericAndSymbols,
            Name = "Alpha-Numeric + Symbols",
            Chars = $"{_lettersCharSet}{_numbersCharSet}{_symbolsCharSet}"
        },
        new()
        {
            Type = CharSetType.AlphaNumeric,
            Name = "Alpha-Numeric",
            Chars = $"{_lettersCharSet}{_numbersCharSet}"
        },
        new()
        {
            Type = CharSetType.NumbersAndSymbols,
            Name = "Numbers + Symbols",
            Chars = $"{_numbersCharSet}{_symbolsCharSet}"
        },
        new()
        {
            Type = CharSetType.LettersOnly,
            Name = "Letters Only",
            Chars = _lettersCharSet
        },
        new()
        {
            Type = CharSetType.NumbersOnly,
            Name = "Numbers Only",
            Chars = _numbersCharSet
        },
        new()
        {
            Type = CharSetType.SymbolsOnly,
            Name = "Symbols Only",
            Chars = _symbolsCharSet
        }
    ];

    private static CharSet DefaultCharSet => CharSets.First();

    public IEnumerable<CharSet> GetCharSets()
    {
        logger.LogInformation(nameof(GetCharSets));
        return CharSets;
    }

    public CharSet GetDefaultCharSet()
    {
        logger.LogInformation(nameof(GetDefaultCharSet));
        return DefaultCharSet;
    }

    public CharSet GetCharSetByType(CharSetType type)
    {
        logger.LogInformation($"{nameof(GetCharSetByType)} | type: {{type}}", type);
        return CharSets.First(x => x.Type == type);
    }
}
