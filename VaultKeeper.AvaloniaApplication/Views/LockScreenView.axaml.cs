using Avalonia.Input;
using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.LockScreen;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class LockScreenView : ViewBase<LockScreenViewModel>
{
    public LockScreenView() => InitializeComponent();

    private void Submit() => Model?.SubmitForm();

    private void SubmitButton_Click(object? sender, RoutedEventArgs e) => Submit();

    private void InputPassword_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            Submit();
    }
}