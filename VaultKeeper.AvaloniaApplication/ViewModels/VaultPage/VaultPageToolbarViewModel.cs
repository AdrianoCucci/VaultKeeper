using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Models;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultPage;

public partial class VaultPageToolbarViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _areInputsVisible = true;

    [ObservableProperty]
    private bool _areInputsEnabled = true;

    [ObservableProperty]
    private string? _searchInput;

    [ObservableProperty]
    private SortDirection _sortInput = SortDirection.Ascending;

    [ObservableProperty]
    private bool _isBulkActionsModeActive = false;

    [ObservableProperty]
    private SelectedItems _selectedItems = SelectedItems.Empty();

    [ObservableProperty]
    private bool _isUngroupSelectedItemsActionVisible = false;

    public void ToggleSortInput() =>
        SortInput = SortInput == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
}
