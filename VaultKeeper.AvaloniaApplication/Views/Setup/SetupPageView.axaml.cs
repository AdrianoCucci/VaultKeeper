using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Setup;

namespace VaultKeeper.AvaloniaApplication.Views.Setup;

public partial class SetupPageView : ViewBase<SetupPageViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> SetupCompletedEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(SetupCompleted), RoutingStrategies.Bubble, typeof(LockScreenPageView));

    public static readonly RoutedEvent<RoutedEventArgs> NavigateBackClickedEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(NavigateBackClicked), RoutingStrategies.Bubble, typeof(LockScreenPageView));


    public event EventHandler<RoutedEventArgs> SetupCompleted { add => AddHandler(SetupCompletedEvent, value); remove => RemoveHandler(SetupCompletedEvent, value); }

    public event EventHandler<RoutedEventArgs> NavigateBackClicked { add => AddHandler(NavigateBackClickedEvent, value); remove => RemoveHandler(NavigateBackClickedEvent, value); }

    public SetupPageView() => InitializeComponent();

    private void NavigateBackButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(NavigateBackClickedEvent, this));

    private void SetupPageStep1View_FormSubmitted(object? sender, RoutedEventArgs e) => Model?.GoToStep2();

    private void SetupPageStep1View_BackupLoaded(object? sender, RoutedEventArgs e) => RaiseEvent(new(SetupCompletedEvent, this));

    private void SetupPageStep2View_Completed(object? sender, RoutedEventArgs e) => RaiseEvent(new(SetupCompletedEvent, this));
}