using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.Forms.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.AvaloniaApplication.Views.VaultPage;
using VaultKeeper.Common.Exceptions;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Common.Results;
using VaultKeeper.Models;
using VaultKeeper.Models.Errors;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.Groups.Extensions;
using VaultKeeper.Models.Settings;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Models.VaultItems.Extensions;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.Groups;
using VaultKeeper.Services.Abstractions.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultPage;

public partial class VaultPageViewModel(
    IVaultItemService vaultItemService,
    IGroupService groupService,
    ISecurityService securityService,
    IPlatformService platformService,
    IKeyGeneratorService keyGeneratorService,
    IErrorReportingService errorReportingService,
    IServiceProvider serviceProvider) : ViewModelBase
{
    public VaultPageToolbarViewModel ToolbarVM { get; } = new();

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsEmpty), nameof(EmptyTemplateTitle), nameof(EmptyTemplateDescription))]
    private ObservableGroupedVaultItemViewModels _groupedVaultItems = [];

    [ObservableProperty]
    private ObservableCollection<VaultItem> _selectedItems = [];

    [ObservableProperty]
    private bool _isSidePaneOpen = false;

    [ObservableProperty]
    private string? _sidePaneTitle;

    [ObservableProperty]
    private object? _sidePaneContent;

    [ObservableProperty]
    private bool _isOverlayVisible;

    [ObservableProperty]
    private object? _overlayContent;

    public bool IsEmpty => _vaultItemData.TotalCount < 1 && _groupData.TotalCount < 1;
    public string EmptyTemplateTitle => IsEmpty ? "No Keys Created" : "No Keys Found";
    public string EmptyTemplateDescription => IsEmpty ? "Create your first key or import your existing keys to get started." : "Search returned no results.";
    public SelectionMode ItemSelectionMode => SelectedItems.Count > 0 ? SelectionMode.Always : SelectionMode.OnFocus;

    private CountedData<VaultItem> _vaultItemData = new();
    private CountedData<Group> _groupData = new();

    public virtual async Task LoadDataAsync(bool refreshUI = true)
    {
        await Task.WhenAll(
        [
            Task.Run(() => LoadVaultItemsAsync(false)),
            Task.Run(() => LoadGroupsAsync(false))
        ]);

        ToolbarVM.AreInputsVisible = !IsEmpty;

        if (refreshUI)
            UpdateMainContent();
    }

    public async Task<CountedData<VaultItem>> LoadVaultItemsAsync(bool refreshUI = true)
    {
        Result<CountedData<VaultItem>> loadResult = await vaultItemService.GetManyCountedAsync();
        if (!loadResult.IsSuccessful)
            throw new Exception($"{nameof(VaultPageViewModel)}: Failed to load items - {loadResult.Message}", loadResult.Exception);

        _vaultItemData = loadResult.Value!;

        ToolbarVM.AreInputsVisible = !IsEmpty;

        if (refreshUI)
            UpdateMainContent();

        return _vaultItemData;
    }

    public async Task<CountedData<Group>> LoadGroupsAsync(bool refreshUI = true)
    {
        Result<CountedData<Group>> loadResult = await groupService.GetManyCountedAsync();
        if (!loadResult.IsSuccessful)
            throw new Exception($"{nameof(VaultPageViewModel)}: Failed to load groups - {loadResult.Message}", loadResult.Exception);

        _groupData = loadResult.Value!;

        ToolbarVM.AreInputsVisible = !IsEmpty;

        if (refreshUI)
            UpdateMainContent();

        return _groupData;
    }

    public async Task HandleToolbarActionAsync(VaultPageToolbarEventArgs eventArgs)
    {
        VaultPageToolbarViewModel viewModel = eventArgs.ViewModel;

        switch (eventArgs.Action)
        {
            case VaultPageToolbarAction.SearchInput:
                await LoadDataAsync(false);
                UpdateMainContent(search: viewModel.SearchInput);
                break;
            case VaultPageToolbarAction.Sort:
                UpdateMainContent(sortDirection: viewModel.SortInput);
                break;
            case VaultPageToolbarAction.AddItem:
                ShowVaultItemCreateForm();
                break;
            case VaultPageToolbarAction.ImportItems:
                // TODO;
                break;
            case VaultPageToolbarAction.ExportItems:
                // TODO;
                break;
            case VaultPageToolbarAction.SelectAllItems:
                SetAllItemsSelected(true);
                break;
            case VaultPageToolbarAction.DeselectAllItems:
                SetAllItemsSelected(false);
                break;
            case VaultPageToolbarAction.GroupSelectedItems:
                // TODO;
                break;
            case VaultPageToolbarAction.ExportSelectedItems:
                // TODO;
                break;
            case VaultPageToolbarAction.DeleteSelectedItems:
                // TODO;
                break;
        }
    }

    public async Task HandleItemActionAsync(VaultItemActionEventArgs eventArgs)
    {
        VaultItemViewModelBase viewModel = eventArgs.ViewModel;
        VaultItem model = viewModel.Model;

        switch (eventArgs.Action)
        {
            case VaultItemAction.CopyName:
                await platformService.GetClipboard().SetTextAsync(model.Name);
                break;
            case VaultItemAction.CopyValue:
                await platformService.GetClipboard().SetTextAsync(Decrypt(model.Value));
                break;
            case VaultItemAction.Edit:
                ShowVaultItemEditForm(model);
                break;
            case VaultItemAction.Delete:
                ShowOverlay(new ConfirmPromptViewModel()
                {
                    Header = "Delete Key",
                    Message = $"Are you sure you want to delete key: \"{viewModel.Model.Name}\"?",
                    CancelAction = HideOverlay,
                    ConfirmAction = async () =>
                    {
                        if (await DeleteVaultItemAsync(viewModel.Model))
                            HideOverlay();
                    }
                });
                break;
            case VaultItemAction.Select:
                {
                    if (SelectedItems.Any(x => x.Id == viewModel.Model.Id)) break;

                    if (SelectedItems.Count < 1)
                    {
                        HideAllForms();
                        UpdateAllItemsSelectionMode(SelectionMode.Always);
                    }

                    SetSelectedItems([.. SelectedItems, viewModel.Model]);

                    break;
                }
            case VaultItemAction.Deselect:
                {
                    VaultItem? selectedItem = SelectedItems.FirstOrDefault(x => x.Id == viewModel.Model.Id);
                    if (selectedItem == null) break;

                    SetSelectedItems(SelectedItems.Except([selectedItem]));
                    UpdateAllItemsSelectionMode();

                    break;
                }
        }
    }

    public async Task HandleItemFormActionAsync(VaultItemFormActionEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case VaultItemFormAction.Cancel:
                if (eventArgs.ViewModel == SidePaneContent)
                    HideVaultItemCreateForm();
                else
                    HideVaultItemEditForm(eventArgs.ViewModel.Model);
                break;
            case VaultItemFormAction.Submit:
                await SaveVaultItemFormAsync(eventArgs);
                break;
            case VaultItemFormAction.ToggleRevealValue:
                ToggleRevealFormItemValue(eventArgs.ViewModel);
                break;
            case VaultItemFormAction.GenerateValue:
                GenerateValueForFormItem(eventArgs.ViewModel);
                break;
        }
    }

    public async Task HandleGroupActionAsnc(GroupActionEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case GroupAction.AddItem:
                ShowVaultItemCreateForm(new() { GroupId = eventArgs.Group.Id });
                break;
            case GroupAction.Edit:
                TryUpdateGroupViewModel(eventArgs.Group, () => new GroupFormViewModel(eventArgs.Group, FormMode.Edit));
                break;
            case GroupAction.CancelEdit:
                TryUpdateGroupViewModel(eventArgs.Group, () => new GroupViewModel(eventArgs.Group));
                break;
            case GroupAction.ConfirmEdit:
                {
                    Result<Group> updateResult = await groupService.UpdateAsync(eventArgs.Group);
                    if (!updateResult.IsSuccessful)
                    {
                        ShowOverlay(new PromptViewModel
                        {
                            Header = "Failed to Update Group",
                            Message = updateResult.Message ?? "An unknown error occurred"
                        });
                        return;
                    }

                    TryUpdateGroupViewModel(eventArgs.Group, () => new GroupViewModel(updateResult.Value!));
                    await LoadGroupsAsync();
                }
                break;
            case GroupAction.Delete:
                {
                    DeleteGroupConfirmPromptViewModel promptVM = new()
                    {
                        Header = "Delete Group",
                        Message = $"Choose what should happen to the keys inside of group: \"{eventArgs.Group.Name}\":",
                        CascadeDeleteMode = CascadeDeleteMode.OrphanChildren,
                        CancelAction = HideOverlay
                    };
                    promptVM.ConfirmAction = async () =>
                    {
                        if (await DeleteGroupAsync(eventArgs.Group, promptVM.CascadeDeleteMode))
                            HideOverlay();
                    };

                    ShowOverlay(promptVM);
                }
                break;
        }
    }

    public void SetAllItemsSelected(bool isSelected)
    {
        IEnumerable<VaultItemShellViewModel> itemVMs = GetVaultItemViewModels();

        foreach (VaultItemShellViewModel item in itemVMs)
        {
            item.IsSelected = isSelected;
        }

        SelectedItems = isSelected ? [.. itemVMs.Select(x => x.Model)] : [];
    }

    public void ShowVaultItemCreateForm(VaultItem? vaultItem = null)
    {
        SidePaneContent = null;
        SidePaneContent = CreateVaultItemFormViewModel(vaultItem ?? new(), FormMode.New);
        SidePaneTitle = "New Key";
        IsSidePaneOpen = true;
        ToolbarVM.AreInputsEnabled = false;
    }

    public void HideVaultItemCreateForm(bool clearData = false)
    {
        IsSidePaneOpen = false;
        ToolbarVM.AreInputsEnabled = true;

        if (clearData)
            SidePaneContent = null;
    }

    public void HideAllForms()
    {
        HideVaultItemCreateForm();

        IEnumerable<VaultItemFormViewModel> itemFormVMs = [..GetVaultItemViewModels()
            .Where(x => x.Content is VaultItemFormViewModel)
            .Select(x => (x.Content as VaultItemFormViewModel)!)];

        foreach (VaultItemFormViewModel vm in itemFormVMs)
        {
            HideVaultItemEditForm(vm.Model);
        }
    }

    public void ShowOverlay(object content)
    {
        OverlayContent = content;
        IsOverlayVisible = true;
    }

    public void HideOverlay()
    {
        OverlayContent = null;
        IsOverlayVisible = false;
    }

    private void UpdateMainContent(
        IEnumerable<VaultItem>? vaultItemData = null,
        IEnumerable<Group>? groupData = null,
        string? search = null,
        SortDirection? sortDirection = null)
    {
        vaultItemData ??= _vaultItemData.Items;
        groupData ??= _groupData.Items;
        search ??= ToolbarVM.SearchInput;
        sortDirection ??= ToolbarVM.SortInput;

        IEnumerable<GroupedVaultItemsViewModel> existingGroupedItemVMs = GroupedVaultItems.AsEnumerable();
        IEnumerable<GroupedVaultItemsViewModel> updatedGroupItemVMs = [];

        Dictionary<Guid, IEnumerable<VaultItem>> groupItemsDict = vaultItemData
            .GroupBy(x => x.GroupId)
            .ToDictionary(x => x.Key ?? Guid.Empty, x => x.AsEnumerable());

        foreach ((Guid groupId, IEnumerable<VaultItem> items) in groupItemsDict)
        {
            Group? group = _groupData.Items.FirstOrDefault(x => x.Id == groupId);
            IEnumerable<VaultItem> groupedItems = items;

            if (!string.IsNullOrWhiteSpace(search))
            {
                if (group == null)
                {
                    groupedItems = groupedItems.Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
                    if (!groupedItems.Any())
                        continue;
                }
                else
                {
                    if (!group.Name.Contains(search, StringComparison.OrdinalIgnoreCase) && !groupedItems.Any(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)))
                        continue;
                }
            }

            GroupedVaultItemsViewModel? listItemVM = group == null
                ? existingGroupedItemVMs.FirstOrDefault(x => x.Group == null) ?? new([], null)
                : existingGroupedItemVMs.FirstOrDefault(x => x.Group?.Model.Id == groupId) ?? new([], new(new GroupViewModel(group)));

            IEnumerable<VaultItemShellViewModel> itemVMs = listItemVM.VaultItems.AsEnumerable();

            foreach (VaultItem item in groupedItems)
            {
                VaultItemShellViewModel? itemVM = itemVMs.FirstOrDefault(x => x.Model.Id == item.Id);
                if (itemVM != null)
                    itemVM.Model = item;
                else
                    itemVMs = itemVMs.Append(CreateVaultItemShellViewModel(new VaultItemViewModel(item)));
            }

            IEnumerable<Guid> groupedItemIds = groupedItems.Select(x => x.Id);
            IEnumerable<VaultItemShellViewModel> itemVMsToRemove = itemVMs.Where(x => !groupedItemIds.Contains(x.Model.Id));
            itemVMs = itemVMs.Except(itemVMsToRemove);

            itemVMs = sortDirection == SortDirection.Ascending
                ? itemVMs.OrderBy(x => x.Model.Name)
                : itemVMs.OrderByDescending(x => x.Model.Name);

            listItemVM.VaultItems = [.. itemVMs];

            updatedGroupItemVMs = updatedGroupItemVMs.Append(listItemVM);
        }

        updatedGroupItemVMs = sortDirection == SortDirection.Ascending
            ? updatedGroupItemVMs.OrderBy(x => x.Group?.Model.Name)
            : updatedGroupItemVMs.OrderByDescending(x => x.Group?.Model.Name);

        GroupedVaultItems = [.. updatedGroupItemVMs];
    }

    private void SetSelectedItems(IEnumerable<VaultItem> selectedItems)
    {
        SelectedItems = [.. selectedItems];
        ToolbarVM.SelectedItemsCount = SelectedItems.Count;
        ToolbarVM.IsBulkActionsModeActive = SelectedItems.Count > 0;
    }

    private void ShowVaultItemEditForm(VaultItem vaultItem) =>
        TryUpdateVaultItemViewModel(vaultItem, () => new(CreateVaultItemFormViewModel(vaultItem, FormMode.Edit))
        {
            SelectionMode = SelectionMode.Disabled
        });

    private void HideVaultItemEditForm(VaultItem vaultItem) =>
        TryUpdateVaultItemViewModel(vaultItem, () => new(new VaultItemViewModel(vaultItem))
        {
            SelectionMode = ItemSelectionMode
        });

    private VaultItemShellViewModel CreateVaultItemShellViewModel(VaultItemViewModelBase content) => new(content)
    {
        SelectionMode = ItemSelectionMode,
        IsSelected = SelectedItems.Any(x => x.Id == content.Model.Id)
    };

    private VaultItemFormViewModel CreateVaultItemFormViewModel(VaultItem vaultItem, FormMode formMode)
    {
        IEnumerable<Group> groupOptions = _groupData.Items;
        Group? selectedGroup = groupOptions.FirstOrDefault(x => x.Id == vaultItem.GroupId);

        VaultItemForm form = new(vaultItem, formMode, selectedGroup)
        {
            GroupOptions = groupOptions
        };

        return new VaultItemFormViewModel(form)
        {
            KeyGenerationSettingsVM = serviceProvider.GetRequiredService<KeyGenerationSettingsViewModel>()
        };
    }

    private async Task SaveVaultItemFormAsync(VaultItemFormActionEventArgs formEvent)
    {
        if (formEvent.Form.HasErrors) return;

        VaultItem formModel = formEvent.Form.GetModel();
        bool shouldEncrypt = formEvent.ViewModel.ValueRevealed;

        if (formEvent.Form.WillCreateGroup)
        {
            Result<Group> addGroupResult = await groupService.AddAsync(formEvent.Form.GetGroup().ToNewGroup());
            if (!addGroupResult.IsSuccessful)
            {
                ShowOverlay(new PromptViewModel
                {
                    Header = "Failed to Create Group",
                    Message = addGroupResult.Message ?? "An unknown error occurred"
                });
                return;
            }

            formModel.GroupId = addGroupResult.Value!.Id;
            await LoadGroupsAsync(false);
        }

        void HandleUpsertFailure(Result failedResult)
        {
            if (failedResult.FailureType == ResultFailureType.Conflict)
            {
                string message = $"Another Key named \"{formModel.Name}\" already exists";
                if (formModel.GroupId.HasValue)
                    message += " in this group";

                errorReportingService.ReportError(new()
                {
                    Header = "Key Name Conflict",
                    Message = $"{message}.",
                    Source = ErrorSource.User
                });
            }
            else
            {
                errorReportingService.ReportError(new()
                {
                    Header = $"Failed to {(formEvent.Form.Mode == FormMode.New ? "Add" : "Update")} Key",
                    Message = failedResult.Message ?? "An unknown error occurred."
                });
            }
        }

        switch (formEvent.Form.Mode)
        {
            case FormMode.New:
                {
                    Result<VaultItem> addResult = await vaultItemService.AddAsync(formModel.ToNewVaultItem(), shouldEncrypt);
                    if (!addResult.IsSuccessful)
                    {
                        HandleUpsertFailure(addResult);
                        return;
                    }

                    HideVaultItemCreateForm();
                    break;
                }
            case FormMode.Edit:
                {
                    Result<VaultItem> updateResult = await vaultItemService.UpdateAsync(formModel, shouldEncrypt);
                    if (!updateResult.IsSuccessful)
                    {
                        HandleUpsertFailure(updateResult);
                        return;
                    }

                    formEvent.ViewModel.UpdateModel(_ => updateResult.Value!);
                    HideVaultItemEditForm(formEvent.ViewModel.Model);

                    break;
                }
        }

        await LoadVaultItemsAsync();
    }

    private void ToggleRevealFormItemValue(VaultItemFormViewModel formVM)
    {
        string? value = formVM.Form.Value;

        if (value != null)
        {
            formVM.Form.Value = formVM.ValueRevealed
                ? Encrypt(value)
                : Decrypt(value);
        }

        formVM.ValueRevealed = !formVM.ValueRevealed;
    }

    private void GenerateValueForFormItem(VaultItemFormViewModel formVM)
    {
        KeyGenerationSettings? settings = formVM.KeyGenerationSettingsVM?.GetUpdatedModel();
        if (settings != null)
            formVM.Form.Value = keyGeneratorService.GenerateKey(settings);
    }

    private async Task<bool> DeleteVaultItemAsync(VaultItem vaultItem)
    {
        if (!_vaultItemData.Items.Contains(vaultItem)) return false;

        Result deleteResult = await vaultItemService.DeleteAsync(vaultItem);

        if (!deleteResult.IsSuccessful)
        {
            errorReportingService.ReportError(new()
            {
                Header = "Failed to Delete Key",
                Message = deleteResult.Message ?? "An unknown error occurred.",
                Exception = deleteResult.Exception
            });

            return false;
        }

        if (TryUpdateVaultItemViewModel(vaultItem, () => null))
            await LoadVaultItemsAsync();

        return true;
    }

    private async Task<bool> DeleteGroupAsync(Group group, CascadeDeleteMode cascadeDeleteMode)
    {
        if (!_groupData.Items.Contains(group)) return false;

        Result deleteResult = await groupService.DeleteAsync(group, cascadeDeleteMode);
        if (!deleteResult.IsSuccessful)
        {
            if (deleteResult.Exception is CascadeDeleteException<Group, VaultItem> cascadeDeleteEx)
            {
                string[] childNames = [.. cascadeDeleteEx.ConflictingChildren.Select(x => $"\"{x.Name}\"")];
                errorReportingService.ReportError(new()
                {
                    Header = "Cannot Keep Keys",
                    Message = $"{childNames.Length} Keys in group \"{group.Name}\" have the same names as other existing ungrouped keys:\n\n{string.Join(", ", childNames)}.\n\nThese keys must either be renamed, deleted, or moved to another group.",
                    Source = ErrorSource.User
                });
            }
            else
            {
                errorReportingService.ReportError(new()
                {
                    Header = "Failed to Delete Group",
                    Message = deleteResult.Message ?? "An unknown error occurred."
                });
            }

            return false;
        }

        if (TryUpdateGroupViewModel(group, () => null))
            await LoadDataAsync();

        return true;
    }

    private IEnumerable<VaultItemShellViewModel> GetVaultItemViewModels() => GroupedVaultItems.SelectMany(x => x.VaultItems);

    private void UpdateAllItemsSelectionMode(SelectionMode? selectionMode = null)
    {
        selectionMode ??= ItemSelectionMode;
        IEnumerable<VaultItemShellViewModel> itemVMs = GetVaultItemViewModels();

        foreach (VaultItemShellViewModel itemVM in itemVMs)
        {
            itemVM.SelectionMode = selectionMode.Value;

            if (itemVM.Content is VaultItemViewModel normalVM)
            {
                normalVM.IsActionsButtonVisible = selectionMode != SelectionMode.Always;
            }
            else
            {
                itemVM.Content = new VaultItemViewModel(itemVM.Model)
                {
                    IsActionsButtonVisible = selectionMode != SelectionMode.Always
                };
            }
        }
    }

    private bool TryUpdateVaultItemViewModel(VaultItem vaultItem, Func<VaultItemShellViewModel?> newModelFunc)
    {
        GroupedVaultItemsViewModel? listItemVM = GroupedVaultItems.FirstOrDefault(x => x.VaultItems.Any(y => y.Model.Id == vaultItem.Id));
        if (listItemVM == null)
            return false;

        ObservableCollection<VaultItemShellViewModel> itemVMCollection = listItemVM.VaultItems;
        VaultItemShellViewModel? vaultItemVM = itemVMCollection.FirstOrDefault(x => x.Model.Id == vaultItem.Id);
        if (vaultItemVM == null)
            return false;

        VaultItemShellViewModel? newVM = newModelFunc.Invoke();
        if (newVM != null)
            itemVMCollection[itemVMCollection.IndexOf(vaultItemVM)] = newVM;
        else
            itemVMCollection.Remove(vaultItemVM);

        return true;
    }

    private bool TryUpdateGroupViewModel(Group group, Func<GroupViewModelBase?> newModelFunc)
    {
        GroupedVaultItemsViewModel? listItemVM = GroupedVaultItems.FirstOrDefault(x => x.Group?.Model.Id == group.Id);
        if (listItemVM == null)
            return false;

        GroupViewModelBase? newVM = newModelFunc.Invoke();
        if (newVM != null)
        {
            if (listItemVM.Group != null)
                listItemVM.Group.Content = newVM;
            else
                listItemVM.Group = new GroupShellViewModel(newVM);
        }
        else
        {
            GroupedVaultItems.Remove(listItemVM);
        }

        return true;
    }

    private string Encrypt(string value)
    {
        Result<string> result = securityService.Encrypt(value);
        return result.Value ?? value;
    }

    private string Decrypt(string value)
    {
        Result<string> result = securityService.Decrypt(value);
        return result.Value ?? value;
    }
}