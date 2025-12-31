using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemListViewModel : ViewModelBase
{
    private static readonly Lazy<VaultItemListViewModel> _designLazy = new(() =>
    {
        Group group = new() { Id = Guid.Empty, Name = "Item Group Name" };
        IEnumerable<VaultItem> items = Enumerable
            .Range(1, 10)
            .Select(x => new VaultItem
            {
                Name = $"Name {x}",
                Value = $"Value {x}",
                GroupId = group.Id
            });

        GroupViewModel groupVM = new(group);
        ObservableCollection<VaultItemViewModelBase> itemVMs = [.. items.Select(x => new VaultItemViewModel(x))];

        return new(itemVMs, new(groupVM));
    });

    public static VaultItemListViewModel Design => _designLazy.Value;
};
