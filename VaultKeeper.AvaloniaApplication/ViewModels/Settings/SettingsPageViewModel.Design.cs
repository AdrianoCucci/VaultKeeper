using Avalonia.Media;
using System.Linq;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Settings;

public partial class SettingsPageViewModel
{
    public static DesignContext Design { get; } = new();

    public class DesignContext : SettingsPageViewModel
    {
        public DesignContext() : base()
        {
            KeyGenerationSettingsVM = KeyGenerationSettingsViewModel.Design;

            EmptyGroupModeDefinitions =
            [
                new() { Mode = EmptyGroupMode.Keep, Name = "Keep", Description = "Keep Groups." },
                new() { Mode = EmptyGroupMode.Delete, Name = "Delete", Description = "Delete Groups." }
            ];
            CurrentEmptyGroupModeDefinition = EmptyGroupModeDefinitions.First();

            ThemeDefinitions =
            [
                new() { ThemeType = AppThemeType.System, ThemeName = "System", BackgroundBrush = new SolidColorBrush(Colors.DarkGreen), ForegroundBrush = new SolidColorBrush(Colors.LightGreen) },
                new() { ThemeType = AppThemeType.Light, ThemeName = "Light", BackgroundBrush = new SolidColorBrush(Colors.WhiteSmoke), ForegroundBrush = new SolidColorBrush(Colors.DarkGray) },
                new() { ThemeType = AppThemeType.Dark, ThemeName = "Dark", BackgroundBrush = new SolidColorBrush(Colors.DarkGray), ForegroundBrush = new SolidColorBrush(Colors.WhiteSmoke) }
            ];
            CurrentThemeDefinition = ThemeDefinitions.First();

            FontSize = 14;
            MaxBackups = 5;
            BackupDirectory = "/path/to/backup";
            AutoBackupOnLogout = true;
        }

        public override void LoadSavedSettings() { }
    }
}
