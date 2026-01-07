using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.ComponentModel;
using System.Linq;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultPage;
using VaultKeeper.Common.Models.Queries;

namespace VaultKeeper.AvaloniaApplication.Views.VaultPage;

public enum VaultPageToolbarAction
{
    SearchInput,
    Sort,
    AddItem,
    ImportItems,
    ExportItems,
    SelectAllItems,
    DeselectAllItems,
    GroupSelectedItems,
    ExportSelectedItems,
    DeleteSelectedItems
}

public class VaultPageToolbarEventArgs(RoutedEvent routedEvent, object? source = null) : RoutedEventArgs(routedEvent, source)
{
    public required VaultPageToolbarViewModel ViewModel { get; init; }
    public required VaultPageToolbarAction Action { get; init; }
}

public partial class VaultPageToolbarView : ViewBase<VaultPageToolbarViewModel>
{
    public static readonly RoutedEvent<VaultPageToolbarEventArgs> ActionInvokedEvent =
        RoutedEvent.Register<VaultPageToolbarEventArgs>(nameof(ActionInvoked), RoutingStrategies.Bubble, typeof(VaultPageToolbarView));

    public event EventHandler<VaultPageToolbarEventArgs> ActionInvoked { add => AddHandler(ActionInvokedEvent, value); remove => RemoveHandler(ActionInvokedEvent, value); }

    public VaultPageToolbarView() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Model!.PropertyChanged += Model_PropertyChanged;
        UpdateSortClass();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Model!.PropertyChanged -= Model_PropertyChanged;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Model.SortInput))
            UpdateSortClass();
    }

    private void UpdateSortClass()
    {
        if (Model == null) return;

        const string classPrefix = "sort";
        Classes.RemoveAll(Classes.Where(x => x.StartsWith(classPrefix)));
        Classes.Add($"{classPrefix}-{Model.SortInput.ToString().ToLower()}");
    }

    private void RaiseEvent(VaultPageToolbarAction action)
    {
        if (Model == null) return;

        RaiseEvent(new VaultPageToolbarEventArgs(ActionInvokedEvent, this)
        {
            ViewModel = Model,
            Action = action
        });
    }

    private async void SearchBox_Debounce(object? sender, TextInputEventArgs e) => RaiseEvent(VaultPageToolbarAction.SearchInput);

    private void SortButton_Click(object? sender, RoutedEventArgs e)
    {
        Model?.ToggleSortInput();
        RaiseEvent(VaultPageToolbarAction.Sort);
    }

    private void ButtonNewKey_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultPageToolbarAction.AddItem);

    private void ButtonImportKeys_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultPageToolbarAction.ImportItems);

    private void ButtonExportKeys_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultPageToolbarAction.ExportItems);

    private void SelectAllButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultPageToolbarAction.SelectAllItems);

    private void DeselectAllButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultPageToolbarAction.DeselectAllItems);

    private void GroupSelected_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultPageToolbarAction.GroupSelectedItems);

    private void ExportSelected_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultPageToolbarAction.ExportSelectedItems);

    private void DeleteSelected_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultPageToolbarAction.DeleteSelectedItems);
}