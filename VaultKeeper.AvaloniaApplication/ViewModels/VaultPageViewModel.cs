using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.Forms.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.Groups.Extensions;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Models.VaultItems.Extensions;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class VaultPageViewModel(
    IVaultItemService vaultItemService,
    IGroupService groupService,
    ISecurityService securityService,
    IPlatformService platformService) : ViewModelBase
{
    [ObservableProperty]
    private ObservableVaultItemViewModels _groupedVaultItems = [];

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsEmpty), nameof(IsToolbarVisible))]
    private object? _mainContent;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsToolbarVisible))]
    private bool _isSidePaneOpen = false;

    [ObservableProperty]
    private string? _sidePaneTitle;

    [ObservableProperty]
    private object? _sidePaneContent;

    public bool IsEmpty => GroupedVaultItems.Count < 1;
    public bool IsToolbarVisible => !IsEmpty && !IsSidePaneOpen;

    private IEnumerable<VaultItem> _vaultItemData = [];
    private IEnumerable<Group> _groupData = [];

    public virtual async Task LoadDataAsync(bool refreshUI = true)
    {
        IEnumerable<VaultItem>? vaultItemData = null;
        IEnumerable<Group>? groupData = null;

        await Task.WhenAll(
        [
            Task.Run(async () => vaultItemData = await LoadVaultItemsAsync(false)),
            Task.Run(async () => groupData = await LoadGroupsAsync(false))
        ]);

        if (refreshUI)
            UpdateMainContent(vaultItemData!, groupData!);
    }

    public async Task<IEnumerable<VaultItem>> LoadVaultItemsAsync(bool refreshUI = true)
    {
        Result<IEnumerable<VaultItem>> loadResult = await vaultItemService.GetManyAsync();
        if (!loadResult.IsSuccessful)
            throw new Exception($"{nameof(VaultPageViewModel)}: Failed to load items - {loadResult.Message}", loadResult.Exception);

        _vaultItemData = loadResult.Value!;

        if (refreshUI)
            UpdateMainContent(_vaultItemData);

        return _vaultItemData;
    }

    public async Task<IEnumerable<Group>> LoadGroupsAsync(bool refreshUI = true)
    {
        Result<IEnumerable<Group>> loadResult = await groupService.GetManyAsync();
        if (!loadResult.IsSuccessful)
            throw new Exception($"{nameof(VaultPageViewModel)}: Failed to load groups - {loadResult.Message}", loadResult.Exception);

        _groupData = loadResult.Value!;

        if (refreshUI)
            UpdateMainContent(groupData: _groupData);

        return _groupData;
    }

    public void ShowVaultItemCreateForm(VaultItem? vaultItem = null)
    {
        SidePaneContent = null;
        SidePaneContent = CreateVaultItemFormViewModel(vaultItem ?? new(), FormMode.New);
        SidePaneTitle = "New Key";
        IsSidePaneOpen = true;
    }

    public void HideVaultItemCreateForm(bool clearData = false)
    {
        IsSidePaneOpen = false;

        if (clearData)
            SidePaneContent = null;
    }

    public void ShowVaultItemEditForm(VaultItemViewModelBase vaultItemVM) =>
        TryUpdateVaultItemViewModel(vaultItemVM.Model, () => CreateVaultItemFormViewModel(vaultItemVM.Model, FormMode.Edit));

    public void HideVaultItemEditForm(VaultItemFormViewModel vaultItemVM) =>
        TryUpdateVaultItemViewModel(vaultItemVM.Model, () => new VaultItemViewModel(vaultItemVM.Model));

    public async Task DeleteVaultItemAsync(VaultItemViewModelBase vaultItemVM)
    {
        VaultItem? item = _vaultItemData.FirstOrDefault(x => x.Id == vaultItemVM.Model.Id);
        if (item == null)
            return;

        Result deleteResult = await vaultItemService.DeleteAsync(item);
        if (!deleteResult.IsSuccessful)
        {
            // TODO: Handle error.
            return;
        }

        if (TryUpdateVaultItemViewModel(vaultItemVM.Model, () => null))
            await LoadVaultItemsAsync();
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
                ShowVaultItemEditForm(viewModel);
                break;
            case VaultItemAction.Delete:
                await DeleteVaultItemAsync(viewModel);
                break;
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
                    HideVaultItemEditForm(eventArgs.ViewModel);
                break;
            case VaultItemFormAction.Submit:
                await SaveVaultItemFormAsync(eventArgs);
                break;
            case VaultItemFormAction.ToggleRevealValue:
                ToggleRevealFormItemValue(eventArgs.ViewModel);
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
                    var updateResult = await groupService.UpdateAsync(eventArgs.Group);
                    if (!updateResult.IsSuccessful)
                    {
                        // TODO: Handle error.
                        return;
                    }

                    TryUpdateGroupViewModel(eventArgs.Group, () => new GroupViewModel(updateResult.Value!));
                    await LoadGroupsAsync();
                }
                break;
            case GroupAction.Delete:
                {
                    // TODO Prompt user to delete with or without child items.
                    var deleteResult = await groupService.DeleteAsync(eventArgs.Group);
                    if (!deleteResult.IsSuccessful)
                    {
                        // TODO: Handle error.
                        return;
                    }

                    TryUpdateGroupViewModel(eventArgs.Group, () => null);
                    await LoadGroupsAsync();
                }
                break;
        }
    }

    public async Task SaveVaultItemFormAsync(VaultItemFormActionEventArgs formEvent)
    {
        if (formEvent.Form.HasErrors) return;

        VaultItem formModel = formEvent.Form.GetModel();
        bool shouldEncrypt = formEvent.ViewModel.ValueRevealed;

        if (formEvent.Form.WillCreateGroup)
        {
            Result<Group> addGroupResult = await groupService.AddAsync(formEvent.Form.GetGroup().ToNewGroup());
            if (!addGroupResult.IsSuccessful)
            {
                // TODO: Handle error.
                return;
            }

            formModel.GroupId = addGroupResult.Value!.Id;
            await LoadGroupsAsync(false);
        }

        switch (formEvent.Form.Mode)
        {
            case FormMode.New:
                {
                    Result<VaultItem> addResult = await vaultItemService.AddAsync(formModel.ToNewVaultItem(), shouldEncrypt);
                    if (!addResult.IsSuccessful)
                    {
                        // TODO: Handle error.
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
                        // TODO: Handle error.
                        return;
                    }

                    formEvent.ViewModel.UpdateModel(_ => updateResult.Value!);
                    HideVaultItemEditForm(formEvent.ViewModel);

                    break;
                }
        }

        await LoadVaultItemsAsync();
    }

    public void ToggleRevealFormItemValue(VaultItemFormViewModel formVM)
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

    private void UpdateMainContent(IEnumerable<VaultItem>? vaultItemData = null, IEnumerable<Group>? groupData = null)
    {
        vaultItemData ??= _vaultItemData;
        groupData ??= _groupData;

        VaultItem[] ungroupedVaultItemData = [.. vaultItemData.Where(x => !x.GroupId.HasValue)];
        IEnumerable<Guid> itemDataIds = vaultItemData.Select(x => x.Id);
        IEnumerable<VaultItemListViewModel> existingGroupedItemVMs = GroupedVaultItems.AsEnumerable();
        HashSet<VaultItemListViewModel> updatedGroupItemVMs = [];

        void ProcessItems(IEnumerable<VaultItem> items, VaultItemListViewModel listVM)
        {
            IEnumerable<VaultItemViewModelBase> itemVMs = listVM.VaultItems.AsEnumerable();

            foreach (VaultItem item in items)
            {
                var itemVM = itemVMs.FirstOrDefault(x => x.Model.Id == item.Id);
                if (itemVM != null)
                    itemVM.Model = item;
                else
                    itemVMs = itemVMs.Append(new VaultItemViewModel(item));
            }

            var itemVMsToRemove = itemVMs.Where(x => !itemDataIds.Contains(x.Model.Id));

            listVM.VaultItems = [.. itemVMs.Except(itemVMsToRemove).OrderBy(x => x.Model.Name)];
        }

        if (ungroupedVaultItemData.Length < 1)
        {
            IEnumerable<VaultItemListViewModel> ungroupedVMs = existingGroupedItemVMs.Where(x => !x.HasGroup);
            existingGroupedItemVMs = existingGroupedItemVMs.Except(ungroupedVMs);
        }
        else
        {
            VaultItemListViewModel ungroupedListItemVM = existingGroupedItemVMs.FirstOrDefault(x => !x.HasGroup) ?? VaultItemListViewModel.Empty();
            ProcessItems(ungroupedVaultItemData, ungroupedListItemVM);

            updatedGroupItemVMs.Add(ungroupedListItemVM);
        }

        foreach (Group group in groupData)
        {
            IEnumerable<VaultItem> itemsInGroup = vaultItemData.Where(x => x.GroupId == group.Id);

            VaultItemListViewModel groupedItemsVM = existingGroupedItemVMs.FirstOrDefault(x => x.Group?.Model.Id == group.Id) ?? VaultItemListViewModel.Empty(group);
            ProcessItems(itemsInGroup, groupedItemsVM);

            updatedGroupItemVMs.Add(groupedItemsVM);
        }

        GroupedVaultItems = [.. updatedGroupItemVMs.OrderBy(x => x.Group?.Model.Name)];
        MainContent = GroupedVaultItems.Count > 0 ? GroupedVaultItems : EmptyViewModel.Instance;
    }

    private bool TryUpdateVaultItemViewModel(VaultItem vaultItem, Func<VaultItemViewModelBase?> newModelFunc)
    {
        VaultItemListViewModel? listItemVM = GroupedVaultItems.FirstOrDefault(x => x.VaultItems.Any(y => y.Model.Id == vaultItem.Id));
        if (listItemVM == null)
            return false;

        ObservableCollection<VaultItemViewModelBase> itemVMCollection = listItemVM.VaultItems;
        VaultItemViewModelBase? vaultItemVM = itemVMCollection.FirstOrDefault(x => x.Model.Id == vaultItem.Id);
        if (vaultItemVM == null)
            return false;

        VaultItemViewModelBase? newVM = newModelFunc.Invoke();
        if (newVM != null)
            itemVMCollection[itemVMCollection.IndexOf(vaultItemVM)] = newVM;
        else
            itemVMCollection.Remove(vaultItemVM);

        return true;
    }

    private bool TryUpdateGroupViewModel(Group group, Func<GroupViewModelBase?> newModelFunc)
    {
        VaultItemListViewModel? listItemVM = GroupedVaultItems.FirstOrDefault(x => x.Group?.Model.Id == group.Id);
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

    private VaultItemFormViewModel CreateVaultItemFormViewModel(VaultItem vaultItem, FormMode formMode)
    {
        IEnumerable<Group> groupOptions = _groupData;
        Group? selectedGroup = groupOptions.FirstOrDefault(x => x.Id == vaultItem.GroupId);

        return new VaultItemFormViewModel(new(vaultItem, formMode, selectedGroup)
        {
            GroupOptions = groupOptions
        });
    }

    private string Encrypt(string value)
    {
        var result = securityService.Encrypt(value);
        return result.Value ?? value;
    }

    private string Decrypt(string value)
    {
        var result = securityService.Decrypt(value);
        return result.Value ?? value;
    }
}