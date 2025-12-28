using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class SetupView : ViewBase<SetupViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> SetupCompletedEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(SetupCompleted), RoutingStrategies.Bubble, typeof(LockScreenView));

    public event EventHandler<RoutedEventArgs> SetupCompleted { add => AddHandler(SetupCompletedEvent, value); remove => RemoveHandler(SetupCompletedEvent, value); }

    public SetupView() => InitializeComponent();

    private async void SubmitButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model == null) return;
        bool didComplete = await Model.SubmitFormAsync();

        if (didComplete)
            RaiseEvent(new(SetupCompletedEvent));
    }

    private async void ImportDataButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model == null) return;
        bool didComplete = await Model.ImportBackupDataAsync();

        if (didComplete)
            RaiseEvent(new(SetupCompletedEvent));
    }
}