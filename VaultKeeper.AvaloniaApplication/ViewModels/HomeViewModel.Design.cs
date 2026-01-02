using System.Diagnostics.CodeAnalysis;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

[ExcludeFromCodeCoverage]
public partial class HomeViewModel
{
    public static readonly HomeViewModel Design = new()
    {
        TabNavItems =
        [
            new(new()
            {
                Key = nameof(VaultPageViewModel),
                Label = "Vault",
            }),
            new(new()
            {
                Key = nameof(SettingsPageViewModel),
                Label = "Settings",
            }),
        ]
    };
}
