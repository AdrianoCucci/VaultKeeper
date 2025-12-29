using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemFormViewModel : VaultItemViewModelBase
{
    public VaultItemForm Form { get; }

    [ObservableProperty, NotifyPropertyChangedFor(nameof(PasswordChar))]
    private bool _valueRevealed = false;

    [ObservableProperty]
    private bool _isValueRevealToggleVisible = false;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(WillCreateGroup))]
    private string? _groupInputText = null;

    [ObservableProperty]
    private IEnumerable<Group> _groupOptions = [];

    [ObservableProperty, NotifyPropertyChangedFor(nameof(WillCreateGroup))]
    private Group? _selectedGroup = null;

    [ObservableProperty]
    private bool _useVerticalLayout = false;

    public char PasswordChar => ValueRevealed ? '\0' : '*';
    public bool WillCreateGroup => SelectedGroup == null && !string.IsNullOrWhiteSpace(GroupInputText);

    public VaultItemFormViewModel(VaultItem vaultItem, FormMode formMode = FormMode.New) : base(vaultItem)
    {
        Form = new(vaultItem, formMode);

        switch (formMode)
        {
            case FormMode.New:
                IsValueRevealToggleVisible = false;
                ValueRevealed = true;
                break;
            case FormMode.Edit:
                IsValueRevealToggleVisible = true;
                ValueRevealed = false;
                break;
        }
    }
}
