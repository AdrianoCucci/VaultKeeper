using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public abstract partial class VaultItemViewModelBase(VaultItem vaultItem) : ViewModelBase<VaultItem>(vaultItem);