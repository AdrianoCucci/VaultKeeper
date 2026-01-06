using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.Models;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemViewModel(VaultItem vaultItem) : VaultItemViewModelBase(vaultItem)
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsSelectable))]
    private bool _isFocused = false;

    [ObservableProperty]
    private bool _optionsMenuOpened = false;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsSelectable))]
    private SelectionMode _selectionMode = SelectionMode.OnFocus;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsSelectable))]
    private bool _isSelected = false;

    public bool IsSelectable =>
        IsSelected ||
        SelectionMode == SelectionMode.Always ||
        (SelectionMode == SelectionMode.OnFocus && IsFocused);
}
