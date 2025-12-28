using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemFormViewModel : VaultItemViewModelBase
{
    public const char PASSWORD_CHAR = '*';

    public VaultItemForm Form { get; }

    [ObservableProperty]
    private bool _valueRevealed = false;

    [ObservableProperty]
    private char? _passwordChar = PASSWORD_CHAR;

    [ObservableProperty]
    private bool _isValueRevealToggleVisible = false;

    [ObservableProperty]
    private bool _useVerticalLayout = false;

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

    public VaultItemFormViewModel() : this(new()) { }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ValueRevealed))
            PasswordChar = ValueRevealed ? '\0' : PASSWORD_CHAR;

        base.OnPropertyChanged(e);
    }
}
