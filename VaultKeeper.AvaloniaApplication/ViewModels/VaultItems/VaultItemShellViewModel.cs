using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.Models;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemShellViewModel(VaultItemViewModelBase content) : ViewModelBase
{
    [ObservableProperty]
    private VaultItemViewModelBase _content = content;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsSelectable))]
    private bool _isFocused = false;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsSelectable))]
    private SelectionMode _selectionMode = SelectionMode.OnFocus;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsSelectable))]
    private bool _isSelected = false;

    public VaultItem Model { get => Content.Model; set => Content.Model = value; }

    public bool IsSelectable =>
        IsSelected ||
        SelectionMode == SelectionMode.Always ||
        (SelectionMode == SelectionMode.OnFocus && IsFocused);
}
