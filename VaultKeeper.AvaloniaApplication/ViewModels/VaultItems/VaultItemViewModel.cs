using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemViewModel(VaultItem vaultItem) : VaultItemViewModelBase(vaultItem)
{
    [ObservableProperty]
    private VaultItemViewModelBase _content = new VaultItemReadOnlyViewModel(vaultItem);

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(ViewMode))
        {
            Content = ViewMode switch
            {
                RecordViewMode.Edit => new VaultItemFormViewModel(Model, FormMode.Edit),
                _ => new VaultItemReadOnlyViewModel(Model)
            };
        }
    }
}
