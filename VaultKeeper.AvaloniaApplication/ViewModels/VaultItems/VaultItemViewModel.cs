using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemViewModel(VaultItem vaultItem) : VaultItemViewModelBase(vaultItem)
{
    [ObservableProperty]
    private bool _isFocused = false;

    [ObservableProperty]
    private bool _isActionsButtonVisible = true;

    [ObservableProperty]
    private bool _optionsMenuOpened = false;
}
