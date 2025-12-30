using Avalonia;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemListViewModel(ObservableCollection<VaultItemViewModelBase> vaultItems, GroupShellViewModel? group) : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<VaultItemViewModelBase> _vaultItems = vaultItems;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasGroup), nameof(BorderThickness))]
    private GroupShellViewModel? _group = group;

    [ObservableProperty]
    private ScrollBarVisibility _horizontalScrollBarVisibility = ScrollBarVisibility.Auto;

    [ObservableProperty]
    private ScrollBarVisibility _verticalScrollBarVisibility = ScrollBarVisibility.Auto;

    [ObservableProperty]
    private object? _emptyTemplate;

    public bool HasGroup => Group != null;
    public Thickness? BorderThickness => HasGroup ? new(4, 0, 0, 0) : null;
}
