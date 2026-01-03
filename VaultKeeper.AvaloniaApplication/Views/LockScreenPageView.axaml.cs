using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class LockScreenPageView : ViewBase<LockScreenPageViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> LoginSuccessEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(LoginSuccess), RoutingStrategies.Bubble, typeof(LockScreenPageView));
    public static readonly RoutedEvent<RoutedEventArgs> ForgotPasswordClickedEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(ForgotPasswordClicked), RoutingStrategies.Bubble, typeof(LockScreenPageView));

    public event EventHandler<RoutedEventArgs> LoginSuccess { add => AddHandler(LoginSuccessEvent, value); remove => RemoveHandler(LoginSuccessEvent, value); }

    public event EventHandler<RoutedEventArgs> ForgotPasswordClicked { add => AddHandler(ForgotPasswordClickedEvent, value); remove => RemoveHandler(ForgotPasswordClickedEvent, value); }

    public LockScreenPageView() => InitializeComponent();

    private async Task SubmitAsync()
    {
        if (Model == null) return;

        bool loginSuccess = await Model.SubmitFormAsync();
        if (loginSuccess)
            RaiseEvent(new(LoginSuccessEvent, this));
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        Model?.Initialize();
        InputPassword.Focus();
    }

    private async void SubmitButton_Click(object? sender, RoutedEventArgs e) => await SubmitAsync();

    private async void InputPassword_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await SubmitAsync();
    }

    private void ForgotPasswordButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(ForgotPasswordClickedEvent, this));
}