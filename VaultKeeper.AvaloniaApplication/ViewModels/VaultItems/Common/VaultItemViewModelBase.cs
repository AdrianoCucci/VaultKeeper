using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

public abstract class VaultItemViewModelBase(VaultItem vaultItem) : ViewModelBase<VaultItem>(vaultItem);