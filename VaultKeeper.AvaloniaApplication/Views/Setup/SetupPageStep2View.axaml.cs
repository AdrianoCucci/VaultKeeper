using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Setup;

namespace VaultKeeper.AvaloniaApplication.Views.Setup;

public partial class SetupPageStep2View : ViewBase<SetupPageStep2ViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> CompletedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(Completed), RoutingStrategies.Bubble, typeof(SetupPageStep2View));

    public event EventHandler<RoutedEventArgs> Completed { add => AddHandler(CompletedEvent, value); remove => RemoveHandler(CompletedEvent, value); }

    public SetupPageStep2View() => InitializeComponent();

    private void SkipButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(CompletedEvent, this));

    private void EncryptionKeyFileView_KeyFileReferenceAdded(object? sender, RoutedEventArgs e) => RaiseEvent(new(CompletedEvent, this));

    private async void EncryptionKeyFileView_GenerateKeyFileClicked(object? sender, RoutedEventArgs e)
    {
        if (Model != null && await Model.EncryptionKeyFileVM.GenerateKeyFileAsync())
            RaiseEvent(new(CompletedEvent, this));
    }

    private async void EncryptionKeyFileView_SelectKeyFileClicked(object? sender, RoutedEventArgs e)
    {
        if (Model != null && await Model.EncryptionKeyFileVM.SelectKeyFileAsync())
            RaiseEvent(new(CompletedEvent, this));
    }
}