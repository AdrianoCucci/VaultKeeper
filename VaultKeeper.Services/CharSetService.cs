using Microsoft.Extensions.Logging;
using System;
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

    private static readonly Dictionary<CharSetType, IEnumerable<char>> _charsDict = new()
    {
        { CharSetType.AlphaNumericAndSymbols, _lettersCharSet.Concat(_numbersCharSet).Concat(_symbolsCharSet) },
        { CharSetType.AlphaNumeric, _lettersCharSet.Concat(_numbersCharSet) },
        { CharSetType.NumbersAndSymbols, _lettersCharSet.Concat(_numbersCharSet).Concat(_symbolsCharSet) },
        { CharSetType.LettersOnly, _lettersCharSet },
        { CharSetType.NumbersOnly, _numbersCharSet },
        { CharSetType.SymbolsOnly, _symbolsCharSet },
        { CharSetType.Custom, string.Empty }
    };

    private static readonly Lazy<CharSet> _defaultCharSetLazy = new(() =>
    {
        CharSetType charSetType = CharSetType.AlphaNumericAndSymbols;
        return new()
        {
            Type = charSetType,
            Chars = _charsDict[charSetType]
        };
    });

    public IEnumerable<CharSet> GetAllCharSets()
    {
        logger.LogInformation(nameof(GetAllCharSets));
        return _charsDict.Select(kvp => new CharSet
        {
            Type = kvp.Key,
            Chars = kvp.Value
        });
    }

    public CharSet GetDefaultCharSet()
    {
        logger.LogInformation(nameof(GetDefaultCharSet));
        return _defaultCharSetLazy.Value;
    }

    public CharSet GetCharSetByType(CharSetType type)
    {
        logger.LogInformation($"{nameof(GetCharSetByType)} | type: {{type}}", type);
        return new()
        {
            Type = type,
            Chars = _charsDict[type]
        };
    }
}
