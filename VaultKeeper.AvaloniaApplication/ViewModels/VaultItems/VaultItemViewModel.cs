using CommunityToolkit.Mvvm.ComponentModel;
using System;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemViewModel(VaultItem vaultItem) : ViewModelBase<VaultItem>(vaultItem)
{
    [ObservableProperty]
    private Guid? _id = vaultItem.Id;

    [ObservableProperty]
    private string _name = vaultItem.Name;

    [ObservableProperty]
    private string _value = vaultItem.Value;

    [ObservableProperty]
    private Guid? _groupId = vaultItem.GroupId;

    [ObservableProperty]
    private bool _isReadOnly = true;

    [ObservableProperty]
    private bool _isPointerEntered = false;

    [ObservableProperty]
    private bool _hasFocus = false;

    [ObservableProperty]
    private bool _valueRevealed = false;

    [ObservableProperty]
    private int _nameColumnSpan = 2;

    [ObservableProperty]
    private int _pointerEnteredContentOpacity;
}