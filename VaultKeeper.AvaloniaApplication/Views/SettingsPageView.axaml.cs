using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class SettingsPageView : ViewBase<SettingsPageViewModel>
{
    public SettingsPageView() => InitializeComponent();

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
}