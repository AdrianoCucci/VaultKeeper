using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class GroupedVaultItemsViewModel : ViewModelBase
{
    private static readonly Lazy<GroupedVaultItemsViewModel> _designLazy = new(() =>
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

        IEnumerable<VaultItemViewModel> itemVMs = items.Select(x => new VaultItemViewModel(x));
        ObservableCollection<VaultItemShellViewModel> itemShellVMs = [.. itemVMs.Select(x => new VaultItemShellViewModel(x))];

        return new(itemShellVMs, new(groupVM));
    });

    public static GroupedVaultItemsViewModel Design => _designLazy.Value;
};
