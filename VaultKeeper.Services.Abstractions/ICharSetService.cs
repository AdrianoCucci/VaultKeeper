using System.Collections.Generic;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.Services.Abstractions;

public interface ICharSetService
{
    IEnumerable<CharSet> GetAllCharSets();
    CharSet GetDefaultCharSet();
    CharSet GetCharSetByType(CharSetType type);
}