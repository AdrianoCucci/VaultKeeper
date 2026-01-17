using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Setup;

public partial class SetupPageStep2ViewModel : ViewModelBase
{
    [ObservableProperty]
    private EncryptionKeyFileViewModel _encryptionKeyFileVM;

    public SetupPageStep2ViewModel(EncryptionKeyFileViewModel encryptionKeyFileVM) => _encryptionKeyFileVM = encryptionKeyFileVM;

#if DEBUG
    public SetupPageStep2ViewModel() => _encryptionKeyFileVM = new();
#endif
}
