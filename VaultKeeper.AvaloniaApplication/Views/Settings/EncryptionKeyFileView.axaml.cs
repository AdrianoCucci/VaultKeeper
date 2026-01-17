using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;

namespace VaultKeeper.AvaloniaApplication.Views.Settings;

public partial class EncryptionKeyFileView : ViewBase<EncryptionKeyFileViewModel>
{

    public static readonly RoutedEvent<RoutedEventArgs> RemoveKeyReferenceClickedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(RemoveKeyReferenceClicked), RoutingStrategies.Bubble, typeof(EncryptionKeyFileView));

    public event EventHandler<RoutedEventArgs> RemoveKeyReferenceClicked { add => AddHandler(RemoveKeyReferenceClickedEvent, value); remove => RemoveHandler(RemoveKeyReferenceClickedEvent, value); }

    public EncryptionKeyFileView() => InitializeComponent();

    private async void GenerateKeyButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model != null)
            await Model.GenerateKeyFileAsync();
    }

    private async void SelectFilePathButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model != null)
            await Model.SelectKeyFileAsync();
    }

    private async void RemoveKeyReferenceButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(RemoveKeyReferenceClickedEvent, this));
}