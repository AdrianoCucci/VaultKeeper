using Avalonia;
using Avalonia.Controls;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class MainContentView : ViewBase<HomeViewModel>
{
    public MainContentView() => InitializeComponent();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Model?.Initialize();
    }

    private void TabStrip_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Model?.UpdateSelectedTabState();
}