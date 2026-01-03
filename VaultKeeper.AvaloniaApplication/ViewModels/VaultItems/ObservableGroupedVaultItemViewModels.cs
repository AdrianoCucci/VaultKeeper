using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public class ObservableGroupedVaultItemViewModels : ObservableCollection<GroupedVaultItemsViewModel>
{
    public ObservableGroupedVaultItemViewModels() : base() { }

    public ObservableGroupedVaultItemViewModels(IEnumerable<GroupedVaultItemsViewModel> collection) : base(collection) { }
}
