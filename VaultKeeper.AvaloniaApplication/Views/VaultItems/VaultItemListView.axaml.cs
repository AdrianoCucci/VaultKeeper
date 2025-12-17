using Avalonia.Controls;
using System.Diagnostics;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public partial class VaultItemListView : UserControl
{
    public VaultItemListViewModel? Model => DataContext as VaultItemListViewModel;

    public VaultItemListView() => InitializeComponent();

    public void SetIsReadOnly(bool isReadOnly)
    {
        if (Model == null)
            return;

        foreach (var item in Model.VaultItems)
        {
            item.IsReadOnly = isReadOnly;
        }
    }

    private void VaultItemView_RevealValueClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Debug.WriteLine($"REVEAL VALUE: {sender}, {e.Source}");
    }
}