using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class LockScreenView : ViewBase<LockScreenViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> LoginSuccessEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(LoginSuccess), RoutingStrategies.Bubble, typeof(LockScreenView));

    public event EventHandler<RoutedEventArgs> LoginSuccess { add => AddHandler(LoginSuccessEvent, value); remove => RemoveHandler(LoginSuccessEvent, value); }

    public LockScreenView() => InitializeComponent();

    private void Submit()
    {
        if (Model?.SubmitForm() == true)
            RaiseEvent(new(LoginSuccessEvent, this));
    }

    private void SubmitButton_Click(object? sender, RoutedEventArgs e) => Submit();

    private void InputPassword_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            Submit();
    }
}