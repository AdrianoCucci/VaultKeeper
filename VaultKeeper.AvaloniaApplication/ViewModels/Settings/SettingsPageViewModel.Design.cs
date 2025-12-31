using Avalonia.Media;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Settings;

public partial class SettingsPageViewModel : ViewModelBase
{
    public static DesignContext Design { get; } = new()
    {
        Model = new()
        {
            Theme = new()
            {
                ThemeType = AppThemeType.System,
                FontSize = 14
            },
            Backup = new()
            {
                BackupDirectory = "/path/to/backups",
                MaxBackups = 3
            },
            KeyGeneration = new()
            {
                CharSet = new()
                {
                    Type = CharSetType.AlphaNumericAndSymbols,
                    Chars = "abc123!@#"
                },
                MinLength = 10,
                MaxLength = 10
            }
        },
        ThemeDefinitions =
        [
            new() { ThemeType = AppThemeType.System, BackgroundBrush = new SolidColorBrush(Colors.DarkGreen), ForegroundBrush = new SolidColorBrush(Colors.LightGreen) },
            new() { ThemeType = AppThemeType.Light, BackgroundBrush = new SolidColorBrush(Colors.WhiteSmoke), ForegroundBrush = new SolidColorBrush(Colors.DarkGray) },
            new() { ThemeType = AppThemeType.Dark, BackgroundBrush = new SolidColorBrush(Colors.DarkGray), ForegroundBrush = new SolidColorBrush(Colors.WhiteSmoke) },
            new() { ThemeType = AppThemeType.HighContrast, BackgroundBrush = new SolidColorBrush(Colors.Black), ForegroundBrush = new SolidColorBrush(Colors.White) }
        ]
    };

    public class DesignContext() : SettingsPageViewModel(null!, null!, null!, null!)
    {
        public override void Initialize() { }
    }
}
