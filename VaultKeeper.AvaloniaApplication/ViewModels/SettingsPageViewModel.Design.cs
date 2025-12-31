using VaultKeeper.Models.Settings;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class SettingsPageViewModel : ViewModelBase
{
    public static SettingsPageViewModel Design { get; } = new(null!, null!, null!, null!)
    {
        Model = new()
        {
            AppTheme = AppThemeType.Default,
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
        }
    };
}
