using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public abstract class VaultItemViewModelBase(VaultItem vaultItem) : ViewModelBase<VaultItem>(vaultItem);