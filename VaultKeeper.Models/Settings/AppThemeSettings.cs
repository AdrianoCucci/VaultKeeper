namespace VaultKeeper.Models.Settings;

public record AppThemeSettings
{
    public AppThemeType ThemeType { get; set; } = AppThemeType.System;
    public int FontSize { get; set; }
}
