using Avalonia.Media;
using VaultKeeper.AvaloniaApplication.Abstractions.Models;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Settings;

public partial class SettingsPageViewModel : ViewModelBase
{
    public static DesignContext Design { get; } = new();

    public class DesignContext : SettingsPageViewModel
    {
        public DesignContext() : base()
        {
            KeyGenerationSettingsVM = KeyGenerationSettingsViewModel.Design;

            AppThemeDefinition[] themeDefinitions =
            [
                new() { ThemeType = AppThemeType.System, ThemeName = "System", BackgroundBrush = new SolidColorBrush(Colors.DarkGreen), ForegroundBrush = new SolidColorBrush(Colors.LightGreen) },
                new() { ThemeType = AppThemeType.Light, ThemeName = "Light", BackgroundBrush = new SolidColorBrush(Colors.WhiteSmoke), ForegroundBrush = new SolidColorBrush(Colors.DarkGray) },
                new() { ThemeType = AppThemeType.Dark, ThemeName = "Dark", BackgroundBrush = new SolidColorBrush(Colors.DarkGray), ForegroundBrush = new SolidColorBrush(Colors.WhiteSmoke) }
            ];

            ThemeDefinitions = themeDefinitions;
            CurrentThemeDefinition = themeDefinitions[0];

            FontSize = 14;
            MaxBackups = 5;
            BackupDirectory = "/path/to/backup";
            AutoBackupOnLogout = true;
        }

        public override void LoadSavedSettings() { }
    }
}
