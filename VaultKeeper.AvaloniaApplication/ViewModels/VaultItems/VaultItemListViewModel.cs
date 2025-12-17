using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemListViewModel(IEnumerable<VaultItem> vaultItems) : ViewModelBase<IEnumerable<VaultItem>>(vaultItems)
{
    public ObservableCollection<VaultItemViewModel> VaultItems { get; } = new(vaultItems.Select(x => new VaultItemViewModel(x)));

    [ObservableProperty]
    public bool _isReadOnly = false;
}
