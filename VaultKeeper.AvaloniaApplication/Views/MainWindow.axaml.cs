using Avalonia.Controls;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private void LockScreenView_LoginSuccess(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.NavigateToHome();
    }
}