using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;

namespace VaultKeeper.AvaloniaApplication.Views.Settings;

public partial class SettingsPageView : ViewBase<SettingsPageViewModel>
{
    public SettingsPageView() => InitializeComponent();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Model?.Initialize();
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

    private void BackupDirectoryInput_TextChanged(object? sender, TextChangedEventArgs e) => Model?.UpdateIsBackupDirectoryInvalid();
}