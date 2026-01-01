namespace VaultKeeper.Models.Settings;

public record AppThemeSettings
{
    public static AppThemeSettings Default => new()
    {
        ThemeType = AppThemeType.System,
        FontSize = 14
    };

    public AppThemeType ThemeType { get; set; }
    public int FontSize { get; set; }
}
