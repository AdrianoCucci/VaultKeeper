using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? Model => DataContext as MainWindowViewModel;

    public MainWindow() => InitializeComponent();

    protected override async void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            await viewModel.InitializeContentAsync();

        base.OnApplyTemplate(e);
    }

    private void SetupView_SetupCompleted(object? sender, RoutedEventArgs e) => Model?.NavigateToLockscreen();

    private async void LockScreenView_LoginSuccess(object? sender, RoutedEventArgs e) => Model?.NavigateToHome();
}