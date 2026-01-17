using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;

namespace VaultKeeper.AvaloniaApplication.Views.Settings;

public partial class EncryptionKeyFileView : ViewBase<EncryptionKeyFileViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> KeyFileReferenceAddedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(KeyFileReferenceAdded), RoutingStrategies.Bubble, typeof(EncryptionKeyFileView));

    public static readonly RoutedEvent<RoutedEventArgs> GenerateKeyFileClickedEvent =
    RoutedEvent.Register<RoutedEventArgs>(nameof(GenerateKeyFileClicked), RoutingStrategies.Bubble, typeof(EncryptionKeyFileView));

    public static readonly RoutedEvent<RoutedEventArgs> SelectKeyFileClickedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(SelectKeyFileClicked), RoutingStrategies.Bubble, typeof(EncryptionKeyFileView));

    public static readonly RoutedEvent<RoutedEventArgs> RemoveKeyReferenceClickedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(RemoveKeyReferenceClicked), RoutingStrategies.Bubble, typeof(EncryptionKeyFileView));

    public static readonly StyledProperty<IBrush?> CardBackgroundProperty = AvaloniaProperty.Register<EncryptionKeyFileView, IBrush?>(nameof(CardBackground));

    public event EventHandler<RoutedEventArgs> KeyFileReferenceAdded { add => AddHandler(KeyFileReferenceAddedEvent, value); remove => RemoveHandler(KeyFileReferenceAddedEvent, value); }

    public event EventHandler<RoutedEventArgs> GenerateKeyFileClicked { add => AddHandler(GenerateKeyFileClickedEvent, value); remove => RemoveHandler(GenerateKeyFileClickedEvent, value); }

    public event EventHandler<RoutedEventArgs> SelectKeyFileClicked { add => AddHandler(SelectKeyFileClickedEvent, value); remove => RemoveHandler(SelectKeyFileClickedEvent, value); }

    public event EventHandler<RoutedEventArgs> RemoveKeyReferenceClicked { add => AddHandler(RemoveKeyReferenceClickedEvent, value); remove => RemoveHandler(RemoveKeyReferenceClickedEvent, value); }

    public IBrush? CardBackground { get => GetValue(CardBackgroundProperty); set => SetValue(CardBackgroundProperty, value); }

    public EncryptionKeyFileView() => InitializeComponent();

    private async void GenerateKeyButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(GenerateKeyFileClickedEvent, this));

    private async void SelectFilePathButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(SelectKeyFileClickedEvent, this));

    private async void RemoveKeyReferenceButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(RemoveKeyReferenceClickedEvent, this));
}