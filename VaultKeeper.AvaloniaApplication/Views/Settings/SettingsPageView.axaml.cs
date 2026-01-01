using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;

namespace VaultKeeper.AvaloniaApplication.Views.Settings;

public partial class SettingsPageView : ViewBase<SettingsPageViewModel>
{
    public SettingsPageView() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Model?.LoadSavedSettings();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Model?.SaveSettings();
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

    private void RestoreDefaultSettingsButton_Click(object? sender, RoutedEventArgs e) => Model?.RestoreDefaultSettings();
}