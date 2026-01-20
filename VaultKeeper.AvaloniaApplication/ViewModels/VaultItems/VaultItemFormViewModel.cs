using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemFormViewModel(VaultItemForm form) : VaultItemViewModelBase(form.OriginalModel)
{
    [ObservableProperty]
    private VaultItemForm _form = form;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsGenerateKeyButtonVisible))]
    private KeyGenerationSettingsViewModel? _keyGenerationSettingsVM;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsGenerateKeyButtonVisible))]
    private bool _isValueReadOnly = false;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(PasswordChar))]
    private bool _valueRevealed = false;

    [ObservableProperty]
    private bool _useVerticalLayout = false;

    public char PasswordChar => ValueRevealed ? '\0' : '*';
    public bool IsGenerateKeyButtonVisible => KeyGenerationSettingsVM != null && !IsValueReadOnly;
}
