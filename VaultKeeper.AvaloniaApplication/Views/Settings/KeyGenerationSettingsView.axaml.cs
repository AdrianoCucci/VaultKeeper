using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;

namespace VaultKeeper.AvaloniaApplication.Views.Settings;

public partial class KeyGenerationSettingsView : ViewBase<KeyGenerationSettingsViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> GenerateButtonClickedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(GenerateButtonClicked), RoutingStrategies.Bubble, typeof(KeyGenerationSettingsView));

    public event EventHandler<RoutedEventArgs> GenerateButtonClicked { add => AddHandler(GenerateButtonClickedEvent, value); remove => RemoveHandler(GenerateButtonClickedEvent, value); }

    public KeyGenerationSettingsView() => InitializeComponent();

    private void GenerateButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(GenerateButtonClickedEvent, this));

    private void Root_LayoutUpdated(object? sender, EventArgs e) => UpdateModel(x => x.UseCompactView = Root.Bounds.Width <= 800);
}