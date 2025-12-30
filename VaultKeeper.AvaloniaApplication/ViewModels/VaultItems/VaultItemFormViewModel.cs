using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemFormViewModel(VaultItemForm form) : VaultItemViewModelBase(form.OriginalModel)
{
    [ObservableProperty]
    private VaultItemForm _form = form;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(PasswordChar))]
    private bool _valueRevealed = form.Mode == FormMode.New;

    [ObservableProperty]
    private bool _isValueRevealToggleVisible = form.Mode == FormMode.Edit;

    [ObservableProperty]
    private bool _useVerticalLayout = false;

    public char PasswordChar => ValueRevealed ? '\0' : '*';
}
