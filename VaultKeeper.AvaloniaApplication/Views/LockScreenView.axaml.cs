using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class LockScreenView : ViewBase<LockScreenViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> LoginSuccessEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(LoginSuccess), RoutingStrategies.Bubble, typeof(LockScreenView));

    public event EventHandler<RoutedEventArgs> LoginSuccess { add => AddHandler(LoginSuccessEvent, value); remove => RemoveHandler(LoginSuccessEvent, value); }

    public LockScreenView() => InitializeComponent();

    private async Task SubmitAsync()
    {
        if (Model == null) return;

        bool loginSuccess = await Model.SubmitFormAsync();
        if (loginSuccess)
            RaiseEvent(new(LoginSuccessEvent, this));
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(InputPassword.Text))
            InputPassword.Clear();

        InputPassword.Focus();
    }

    private async void SubmitButton_Click(object? sender, RoutedEventArgs e) => await SubmitAsync();

    private async void InputPassword_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await SubmitAsync();
    }
}