using Avalonia.Controls;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class MainContentView : ViewBase<MainContentViewModel>
{
    public MainContentView() => InitializeComponent();

    private void TabStrip_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Model?.UpdateSelectedTabState();
}