using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemReadOnlyViewModel(VaultItem vaultItem) : VaultItemViewModelBase(vaultItem)
{
    [ObservableProperty]
    private bool _isFocused = false;

    [ObservableProperty]
    private bool _optionsMenuOpened = false;
}