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
    public SelectionMode ItemSelectionMode => _selectedItems.Count > 0 ? SelectionMode.Always : SelectionMode.OnFocus;

    private CountedData<VaultItem> _vaultItemData = new();
    private CountedData<Group> _groupData = new();
    private HashSet<VaultItem> _selectedItems = [];

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
                SetSelectedItems(_vaultItemData.Items);
                break;
            case VaultPageToolbarAction.DeselectAllItems:
                SetSelectedItems([]);
                break;
            case VaultPageToolbarAction.GroupSelectedItems:
                {
                    GroupSelectInputViewModel groupInputVM = new(_groupData.Items);

                    ShowOverlay(new GroupItemsConfirmPromptViewModel
                    {
                        Header = "Group Keys",
                        Message = $"Select or create a group to place {_selectedItems.Count} keys in.",
                        GroupInputVM = groupInputVM,
                        CancelAction = HideOverlay,
                        ConfirmAction = async _ =>
                        {
                            Group group = groupInputVM.GetGroup();

                            if (groupInputVM.WillCreateGroup)
                            {
                                Result<Group> createGroupResult = await CreateGroupAsync(group);
                                if (!createGroupResult.IsSuccessful)
                                    return;

                                group = createGroupResult.Value!;
                            }

                            Result<IEnumerable<VaultItem>> updateResult = await UpdateVaultItemsAsync(_selectedItems.Select(x => x with { GroupId = group.Id }));
                            if (updateResult.IsSuccessful)
                            {
                                SetSelectedItems([]);
                                await LoadDataAsync();
                                HideOverlay();
                            }
                        }
                    });

                    break;
                }
            case VaultPageToolbarAction.ExportSelectedItems:
                // TODO;
                break;
            case VaultPageToolbarAction.DeleteSelectedItems:
                ShowOverlay(new ConfirmPromptViewModel
                {
                    Header = "Delete Keys",
                    Message = $"Are you sure you want to delete the following {_selectedItems.Count} selected keys?\n\n- {string.Join("\n- ", _selectedItems.Select(x => x.Name))}",
                    CancelAction = HideOverlay,
                    ConfirmAction = async _ =>
                    {
                        Result<long> deleteResult = await DeleteVaultItemsAsync(_selectedItems);

                        if (deleteResult.IsSuccessful)
                        {
                            SetSelectedItems([]);
                            await LoadDataAsync();
                            HideOverlay();
                        }
                    }
                });
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
                    ConfirmAction = async _ =>
                    {
                        if (await DeleteVaultItemAsync(viewModel.Model))
                            HideOverlay();
                    }
                });
                break;
            case VaultItemAction.Select:
                {
                    if (_selectedItems.Any(x => x.Id == viewModel.Model.Id)) break;
                    SetSelectedItems([.. _selectedItems, viewModel.Model]);

                    break;
                }
            case VaultItemAction.Deselect:
                {
                    VaultItem? selectedItem = _selectedItems.FirstOrDefault(x => x.Id == viewModel.Model.Id);
                    if (selectedItem == null) break;

                    SetSelectedItems(_selectedItems.Except([selectedItem]));

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
                    bool groupIsNotEmpty = _vaultItemData.Items.Where(x => x.GroupId == eventArgs.Group.Id).Any();
                    ConfirmPromptViewModel promptVM;

                    if (groupIsNotEmpty)
                    {
                        promptVM = new DeleteGroupConfirmPromptViewModel()
                        {
                            Header = "Delete Group",
                            Message = $"Choose what should happen to the keys inside of group: \"{eventArgs.Group.Name}\":",
                            CascadeDeleteMode = CascadeDeleteMode.OrphanChildren,
                        };
                    }
                    else
                    {
                        promptVM = new ConfirmPromptViewModel
                        {
                            Header = "Delete Group",
                            Message = $"Are you sure you want to delete group: \"{eventArgs.Group.Name}\"?"
                        };
                    }

                    promptVM.CancelAction = HideOverlay;
                    promptVM.ConfirmAction = async vm =>
                    {
                        CascadeDeleteMode cascadeDeleteMode = vm is DeleteGroupConfirmPromptViewModel groupPromptVM
                            ? groupPromptVM.CascadeDeleteMode
                            : CascadeDeleteMode.DeleteChildren;

                        Result deleteResult = await DeleteGroupAsync(eventArgs.Group, cascadeDeleteMode);
                        if (deleteResult.IsSuccessful)
                        {
                            HideOverlay();
                            await LoadDataAsync();
                        }
                    };

                    ShowOverlay(promptVM);
                }
                break;
        }
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

        IEnumerable<Group> emptyGroups = groupData.Where(x => !groupItemsDict.ContainsKey(x.Id));
        foreach (var emptyGroup in emptyGroups)
        {
            groupItemsDict[emptyGroup.Id] = [];
        }

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
        _selectedItems = [.. selectedItems];
        int newSelectedItemsCount = _selectedItems.Count;
        bool hasSelectedItems = newSelectedItemsCount > 0;

        ToolbarVM.SelectedItemsCount = newSelectedItemsCount;
        ToolbarVM.IsBulkActionsModeActive = hasSelectedItems;

        if (hasSelectedItems)
            HideVaultItemCreateForm();

        foreach (GroupedVaultItemsViewModel groupedItemsVM in GroupedVaultItems)
        {
            GroupShellViewModel? groupVM = groupedItemsVM.Group;

            if (groupVM?.Content is GroupViewModel normalGroupVM)
            {
                normalGroupVM.IsReadOnly = hasSelectedItems;
            }
            else if (hasSelectedItems && groupVM != null)
            {
                groupVM.Content = new GroupViewModel(groupVM.Model)
                {
                    IsReadOnly = true
                };
            }

            foreach (var itemVM in groupedItemsVM.VaultItems)
            {
                itemVM.IsSelected = _selectedItems.Contains(itemVM.Model);
                itemVM.SelectionMode = ItemSelectionMode;

                if (itemVM.Content is VaultItemViewModel normalItemVM)
                {
                    normalItemVM.IsReadOnly = hasSelectedItems;
                }
                else if (hasSelectedItems)
                {
                    itemVM.Content = new VaultItemViewModel(itemVM.Model)
                    {
                        IsReadOnly = true
                    };
                }
            }
        }
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
        IsSelected = _selectedItems.Any(x => x.Id == content.Model.Id)
    };

    private VaultItemFormViewModel CreateVaultItemFormViewModel(VaultItem vaultItem, FormMode formMode)
    {
        IEnumerable<Group> groupOptions = _groupData.Items;
        Group? selectedGroup = groupOptions.FirstOrDefault(x => x.Id == vaultItem.GroupId);

        GroupSelectInputViewModel groupInput = new(groupOptions, selectedGroup);
        VaultItemForm form = new(vaultItem, formMode, groupInput);

        return new VaultItemFormViewModel(form)
        {
            KeyGenerationSettingsVM = serviceProvider.GetRequiredService<KeyGenerationSettingsViewModel>()
        };
    }

    private async Task SaveVaultItemFormAsync(VaultItemFormActionEventArgs formEvent)
    {
        if (formEvent.Form.HasErrors) return;

        VaultItem formModel = formEvent.Form.GetModel();
        GroupSelectInputViewModel groupInput = formEvent.Form.GroupInputVM;
        bool shouldEncrypt = formEvent.ViewModel.ValueRevealed;

        if (groupInput.WillCreateGroup)
        {
            Result<Group> createGroupResult = await CreateGroupAsync(groupInput.GetGroup());
            if (!createGroupResult.IsSuccessful)
                return;

            formModel.GroupId = createGroupResult.Value!.Id;
            await LoadGroupsAsync(false);
        }

        switch (formEvent.Form.Mode)
        {
            case FormMode.New:
                {
                    Result<VaultItem> result = await CreateVaultItemAsync(formModel, shouldEncrypt);
                    if (result.IsSuccessful)
                        HideVaultItemCreateForm();

                    break;
                }
            case FormMode.Edit:
                {
                    Result<VaultItem> result = await UpdateVaultItemAsync(formModel, shouldEncrypt);
                    if (result.IsSuccessful)
                    {
                        formEvent.ViewModel.UpdateModel(_ => result.Value!);
                        HideVaultItemEditForm(formEvent.ViewModel.Model);
                    }

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

    private async Task<Result<VaultItem>> CreateVaultItemAsync(VaultItem item, bool shouldEncrypt)
    {
        Result<VaultItem> result = await vaultItemService.AddAsync(item.ToNewVaultItem(), shouldEncrypt);

        if (!result.IsSuccessful)
            ReportFailedVaultItemUpsert(result, FormMode.New);

        return result;
    }

    private async Task<Result<VaultItem>> UpdateVaultItemAsync(VaultItem item, bool shouldEncrypt)
    {
        Result<VaultItem> result = await vaultItemService.UpdateAsync(item, shouldEncrypt);

        if (!result.IsSuccessful)
            ReportFailedVaultItemUpsert(result, FormMode.Edit);

        return result;
    }

    private async Task<Result<IEnumerable<VaultItem>>> UpdateVaultItemsAsync(IEnumerable<VaultItem> vaultItems)
    {
        Result<IEnumerable<VaultItem>> result = await vaultItemService.UpdateManyAsync(vaultItems);

        if (!result.IsSuccessful)
            ReportFailedVaultItemUpsert(result, FormMode.Edit, true);

        return result;
    }

    private void ReportFailedVaultItemUpsert(Result failedResult, FormMode formMode, bool isMultiple = false)
    {
        string header = failedResult.FailureType == ResultFailureType.Conflict
            ? "Key Name Conflict"
            : $"Failed to {(formMode == FormMode.New ? "Add" : "Update")} Key{(isMultiple ? 's' : '\0')}";

        errorReportingService.ReportError(new()
        {
            Header = header,
            Message = failedResult.Message?.Replace("Vault Item", "Key") ?? "An unknown error occurred.",
            Source = failedResult.FailureType == ResultFailureType.Unknown ? ErrorSource.Application : ErrorSource.User
        });
    }

    private async Task<Result<Group>> CreateGroupAsync(Group group)
    {
        Result<Group> result = await groupService.AddAsync(group.ToNewGroup());

        if (!result.IsSuccessful)
        {
            errorReportingService.ReportError(new()
            {
                Header = "Failed to Create Group",
                Message = result.Message ?? "An unknown error occurred",
                Source = ErrorSource.User
            });
        }

        return result;
    }

    private async Task<Result<long>> DeleteVaultItemsAsync(IEnumerable<VaultItem> vaultItems)
    {
        Result<long> deleteResult = await vaultItemService.DeleteManyAsync(vaultItems);

        if (!deleteResult.IsSuccessful)
        {
            errorReportingService.ReportError(new()
            {
                Header = "Failed to Delete Keys",
                Message = deleteResult.Message ?? "An unknown error occurred.",
                Exception = deleteResult.Exception
            });

        }

        return deleteResult;
    }

    private async Task<Result> DeleteGroupAsync(Group group, CascadeDeleteMode cascadeDeleteMode)
    {
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
        }

        return deleteResult;
    }

    private IEnumerable<VaultItemShellViewModel> GetVaultItemViewModels() => GroupedVaultItems.SelectMany(x => x.VaultItems);

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