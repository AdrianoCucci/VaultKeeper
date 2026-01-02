using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;

namespace VaultKeeper.AvaloniaApplication.Views.Settings;

public partial class KeyGenerationSettingsView : ViewBase<KeyGenerationSettingsViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> KeyGeneratedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(KeyGenerated), RoutingStrategies.Bubble, typeof(KeyGenerationSettingsView));

    public event EventHandler<RoutedEventArgs> KeyGenerated { add => AddHandler(KeyGeneratedEvent, value); remove => RemoveHandler(KeyGeneratedEvent, value); }

    public KeyGenerationSettingsView() => InitializeComponent();

    private void GenerateButton_Click(object? sender, RoutedEventArgs e)
    {
        string? key = Model?.GenerateKey();

        if (!string.IsNullOrWhiteSpace(key))
            RaiseEvent(new(KeyGeneratedEvent, this));
    }
}