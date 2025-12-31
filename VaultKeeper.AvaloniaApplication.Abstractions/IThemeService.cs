using System.Collections.Generic;
using VaultKeeper.AvaloniaApplication.Abstractions.Models;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.AvaloniaApplication.Abstractions;

public interface IThemeService
{
    IEnumerable<AppThemeDefinition> GetThemeDefinitions();
    AppThemeDefinition GetThemeDefinitionByType(AppThemeType themeType);
    void SetTheme(AppThemeType themeType);
    void SetBaseFontSize(double fontSize);
}