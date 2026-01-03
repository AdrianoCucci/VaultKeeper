using VaultKeeper.AvaloniaApplication.ViewModels.Settings;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemFormViewModel : VaultItemViewModelBase
{
    public static VaultItemFormViewModel Design { get; } = new(new(new() { Name = "Name", Value = "Value" }))
    {
        KeyGenerationSettingsVM = KeyGenerationSettingsViewModel.Design
    };
}
