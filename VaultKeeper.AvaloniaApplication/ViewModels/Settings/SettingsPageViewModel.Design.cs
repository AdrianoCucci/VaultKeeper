using Avalonia.Media;
using System;
using System.Linq;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Settings;

public partial class SettingsPageViewModel : ViewModelBase
{
    public static DesignContext Design { get; } = new()
    {
        Model = UserSettings.Default,
        ThemeDefinitions =
        [
            new() { ThemeType = AppThemeType.System, ThemeName = "System", BackgroundBrush = new SolidColorBrush(Colors.DarkGreen), ForegroundBrush = new SolidColorBrush(Colors.LightGreen) },
            new() { ThemeType = AppThemeType.Light, ThemeName = "Light", BackgroundBrush = new SolidColorBrush(Colors.WhiteSmoke), ForegroundBrush = new SolidColorBrush(Colors.DarkGray) },
            new() { ThemeType = AppThemeType.Dark, ThemeName = "Dark", BackgroundBrush = new SolidColorBrush(Colors.DarkGray), ForegroundBrush = new SolidColorBrush(Colors.WhiteSmoke) }
        ],
        FontSize = 14,
        MaxBackups = 5,
        BackupDirectory = "/path/to/backup",
        AutoBackupOnShutdown = true,
        CharSets = Enum.GetValues<CharSetType>().Select(x => new CharSet { Type = x, Name = x.ToString(), Chars = $"{x} ABC" }),
        KeyGenMinLength = 10,
        KeyGenMaxLength = 20
    };

    public class DesignContext() : SettingsPageViewModel(null!, null!, null!, null!, null!, null!)
    {
        public override void LoadSavedSettings() { }
    }
}
