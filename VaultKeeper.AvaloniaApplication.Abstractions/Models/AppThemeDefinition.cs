using Avalonia.Media;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.AvaloniaApplication.Abstractions.Models;

public record AppThemeDefinition
{
    public required AppThemeType ThemeType { get; init; }
    public required string ThemeName { get; init; }
    public IBrush? BackgroundBrush { get; init; }
    public IBrush? ForegroundBrush { get; init; }
}
