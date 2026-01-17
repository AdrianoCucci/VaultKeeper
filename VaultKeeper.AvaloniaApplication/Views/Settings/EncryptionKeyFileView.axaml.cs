using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;

namespace VaultKeeper.AvaloniaApplication.Views.Settings;

public partial class EncryptionKeyFileView : ViewBase<EncryptionKeyFileViewModel>
{
    public EncryptionKeyFileView() => InitializeComponent();

    private async void GenerateKeyButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model != null)
            await Model.GenerateKeyFileAsync();
    }

    private async void SelectFilePathButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model != null)
            await Model.SelectKeyFileAsync();
    }
}