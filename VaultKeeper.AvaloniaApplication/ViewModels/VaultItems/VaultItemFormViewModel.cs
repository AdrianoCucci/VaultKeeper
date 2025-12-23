using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemFormViewModel(VaultItem vaultItem, FormMode formMode = FormMode.New) : VaultItemViewModelBase(vaultItem)
{
    public const char PASSWORD_CHAR = '*';
    public VaultItemEditForm Form { get; } = new(vaultItem, formMode);

    [ObservableProperty]
    private bool _valueRevealed = false;

    [ObservableProperty]
    private char? _passwordChar = PASSWORD_CHAR;

    [ObservableProperty]
    private bool _useVerticalLayout = false;

    public VaultItemFormViewModel() : this(new()) { }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ValueRevealed))
            PasswordChar = ValueRevealed ? null : PASSWORD_CHAR;

        base.OnPropertyChanged(e);
    }
}
