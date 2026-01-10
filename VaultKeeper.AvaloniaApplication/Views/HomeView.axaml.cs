using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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

    private async void ChangePasswordButton_Click(object? sender, RoutedEventArgs e) => Model?.ShowChangePasswordForm();

    private async void LogoutButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model != null)
            await Model.LogoutAsync();
    }

    private void PromptView_Acknowledged(object? sender, RoutedEventArgs e) => Model?.HideOverlay();
}