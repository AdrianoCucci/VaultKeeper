using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;

namespace VaultKeeper.AvaloniaApplication.Views.Settings;

public partial class SettingsPageView : ViewBase<SettingsPageViewModel>
{
    public SettingsPageView() => InitializeComponent();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Model?.LoadSavedSettings();

        // For some reason, the ScrollViewer of this page loads scrolled down near the middle. This puts it back to the top.
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Task.Delay(1);
            PART_ScrollViewer.Offset = Vector.Zero;
        });
    }

    private async void SelectDirectoryButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model != null)
            await Model.SetBackupDirectoryFromFolderPickerAsync();
    }

    private async void BackupNowButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model != null)
            await Model.CreateBackupAsync();
    }

    private async void LoadBackupButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model != null)
            await Model.LoadBackupAsync();
    }

    private void RestoreDefaultSettingsButton_Click(object? sender, RoutedEventArgs e) => Model?.RestoreDefaultSettings();
}