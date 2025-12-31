using Avalonia;
using Avalonia.Media;

namespace VaultKeeper.AvaloniaApplication.Views.Settings;

public class SettingsSectionShell : ViewBase
{
    public static readonly StyledProperty<string?> HeaderTextProperty = AvaloniaProperty.Register<SettingsSectionShell, string?>(nameof(HeaderText));

    public static readonly StyledProperty<Geometry?> HeaderIconProperty = AvaloniaProperty.Register<SettingsSectionShell, Geometry?>(nameof(HeaderIcon));

    public string? HeaderText { get => GetValue(HeaderTextProperty); set => SetValue(HeaderTextProperty, value); }
    public Geometry? HeaderIcon { get => GetValue(HeaderIconProperty); set => SetValue(HeaderIconProperty, value); }
}