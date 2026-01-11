using Avalonia.Interactivity;
using System;
using System.Diagnostics;
using VaultKeeper.AvaloniaApplication.ViewModels.Importing;

namespace VaultKeeper.AvaloniaApplication.Views.Importing;

public partial class VaultItemImportView : ViewBase<VaultItemImportViewModel>
{
    public static readonly RoutedEvent<RoutedEventArgs> ImportSuccessEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(ImportSuccess), RoutingStrategies.Bubble, typeof(VaultItemImportView));

    public event EventHandler<RoutedEventArgs> ImportSuccess { add => AddHandler(ImportSuccessEvent, value); remove => RemoveHandler(ImportSuccessEvent, value); }

    public int Rows { get; set; }

    public VaultItemImportView() => InitializeComponent();

    private void Self_LayoutUpdated(object? sender, EventArgs e) => UpdateModel(x => x.ItemsGridRows = Bounds.Width > 600 ? 1 : x.ImportSources.Count);

    private async void ImportButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model is not VaultItemImportViewModel model) return;

        bool importSucccess = await model.SelectAndProcessImportFileAsync();
        if (importSucccess)
            RaiseEvent(new(ImportSuccessEvent, this));
    }
}