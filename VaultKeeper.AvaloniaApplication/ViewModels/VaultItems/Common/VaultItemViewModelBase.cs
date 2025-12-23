using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

public abstract partial class VaultItemViewModelBase(VaultItem vaultItem) : ViewModelBase<VaultItem>(vaultItem)
{
    [ObservableProperty]
    private RecordViewMode _viewMode;
}