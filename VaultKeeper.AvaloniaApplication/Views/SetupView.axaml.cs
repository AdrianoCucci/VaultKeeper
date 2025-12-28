using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class SetupView : ViewBase<SetupViewModel>
{
    public SetupView() => InitializeComponent();

    private void SubmitButton_Click(object? sender, RoutedEventArgs e)
    {
    }

    private async void ImportDataButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model != null)
            await Model.ImportBackupDataAsync();
    }
}