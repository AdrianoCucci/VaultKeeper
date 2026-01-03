using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.Models.Groups;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class GroupedVaultItemsViewModel(ObservableCollection<VaultItemViewModelBase> vaultItems, GroupShellViewModel? group) : ViewModelBase
{
    public static GroupedVaultItemsViewModel Empty(Group? group = null)
    {
        GroupShellViewModel? shellVM = group == null ? null : new(new GroupViewModel(group));
        return new([], shellVM);
    }

    [ObservableProperty]
    private ObservableCollection<VaultItemViewModelBase> _vaultItems = vaultItems;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasGroup), nameof(BorderThickness))]
    private GroupShellViewModel? _group = group;

    public bool HasGroup => Group != null;
    public Thickness? BorderThickness => HasGroup ? new(4, 0, 0, 0) : null;
}
