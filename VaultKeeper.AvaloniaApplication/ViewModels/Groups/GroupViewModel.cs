using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Groups;

public partial class GroupViewModel : ViewModelBase<Group>
{
    [ObservableProperty]
    private ObservableCollection<VaultItemViewModelBase> _vaultItems = [];

    public GroupViewModel(Group group, IEnumerable<VaultItem>? vaultItems = null) : base(group)
    {
        if (vaultItems != null)
            _vaultItems = [.. vaultItems.Select(x => new VaultItemViewModel(x))];
    }
}
