using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class SetupPageView : ViewBase<SetupPageViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> SetupCompletedEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(SetupCompleted), RoutingStrategies.Bubble, typeof(LockScreenPageView));

    public static readonly RoutedEvent<RoutedEventArgs> NavigateBackClickedEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(NavigateBackClicked), RoutingStrategies.Bubble, typeof(LockScreenPageView));


    public event EventHandler<RoutedEventArgs> SetupCompleted { add => AddHandler(SetupCompletedEvent, value); remove => RemoveHandler(SetupCompletedEvent, value); }

    public event EventHandler<RoutedEventArgs> NavigateBackClicked { add => AddHandler(NavigateBackClickedEvent, value); remove => RemoveHandler(NavigateBackClickedEvent, value); }

    public SetupPageView() => InitializeComponent();

    private async void SubmitButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model == null) return;
        bool didComplete = await Model.SubmitFormAsync();

        if (didComplete)
            RaiseEvent(new(SetupCompletedEvent, this));
    }

    private async void ImportDataButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model == null) return;
        bool didComplete = await Model.ImportBackupDataAsync();

        if (didComplete)
            RaiseEvent(new(SetupCompletedEvent, this));
    }

    private void NavigateBackButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(NavigateBackClickedEvent, this));
}