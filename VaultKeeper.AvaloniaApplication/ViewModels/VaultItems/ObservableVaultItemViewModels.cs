using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public class ObservableVaultItemViewModels : ObservableCollection<VaultItemListViewModel>
{
    public ObservableVaultItemViewModels() : base() { }

    public ObservableVaultItemViewModels(IEnumerable<VaultItemListViewModel> collection) : base(collection) { }
}
