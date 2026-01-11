using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Importing;

namespace VaultKeeper.AvaloniaApplication.Views.Importing;

public partial class VaultItemImportView : ViewBase<VaultItemImportViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> ProcessSucceededEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(ProcessSucceeded), RoutingStrategies.Bubble, typeof(VaultItemImportView));

    public event EventHandler<RoutedEventArgs> ProcessSucceeded { add => AddHandler(ProcessSucceededEvent, value); remove => RemoveHandler(ProcessSucceededEvent, value); }

    public int Rows { get; set; }

    public VaultItemImportView() => InitializeComponent();

    private void Self_LayoutUpdated(object? sender, EventArgs e) => UpdateModel(x => x.ItemsGridRows = Bounds.Width > 600 ? 1 : x.Sources.Count);

    private async void Button_Click(object? sender, RoutedEventArgs e)
    {
        if (Model is not VaultItemImportViewModel model) return;

        bool isProcessSuccessful = await model.SelectAndProcessFileAsync();
        if (isProcessSuccessful)
            RaiseEvent(new(ProcessSucceededEvent, this));
    }
}