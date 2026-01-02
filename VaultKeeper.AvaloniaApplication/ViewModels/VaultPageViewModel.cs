using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Constants;
using VaultKeeper.AvaloniaApplication.Extensions;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.Forms.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.Common.Models.Queries;
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
    IPlatformService platformService,
    IApplicationService applicationService) : ViewModelBase
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsEmpty), nameof(EmptyTemplateTitle), nameof(EmptyTemplateDescription))]
    private ObservableVaultItemViewModels _groupedVaultItems = [];

    [ObservableProperty]
    private string? _searchInput;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(SortIcon))]
    private SortDirection _sortInput = SortDirection.Ascending;

    [ObservableProperty]
    private bool _isSidePaneOpen = false;

    [ObservableProperty]
    private string? _sidePaneTitle;

    [ObservableProperty]
    private object? _sidePaneContent;

    public bool IsEmpty => _vaultItemData.TotalCount < 1 && _groupData.TotalCount < 1;
    public Geometry? SortIcon => SortInput == SortDirection.Ascending
        ? applicationService.GetApplication().GetResourceOrDefault<StreamGeometry>(Icons.ArrowUpAZ)
        : applicationService.GetApplication().GetResourceOrDefault<StreamGeometry>(Icons.ArrowDownAZ);
    public string EmptyTemplateTitle => IsEmpty ? "No Keys Created" : "No Keys Found";
    public string EmptyTemplateDescription => IsEmpty ? "Create your first key or import your existing keys to get started." : "Search returned no results.";

    private CountedData<VaultItem> _vaultItemData = new();
    private CountedData<Group> _groupData = new();

    public virtual async Task LoadDataAsync(bool refreshUI = true)
    {
        CountedData<VaultItem>? vaultItemData = null;
        CountedData<Group>? groupData = null;

        await Task.WhenAll(
        [
            Task.Run(async () => vaultItemData = await LoadVaultItemsAsync(false)),
            Task.Run(async () => groupData = await LoadGroupsAsync(false))
        ]);

        if (refreshUI)
            UpdateMainContent();
    }

    public async Task<CountedData<VaultItem>> LoadVaultItemsAsync(bool refreshUI = true)
    {
        Result<CountedData<VaultItem>> loadResult = await vaultItemService.GetManyCountedAsync();
        if (!loadResult.IsSuccessful)
            throw new Exception($"{nameof(VaultPageViewModel)}: Failed to load items - {loadResult.Message}", loadResult.Exception);

        _vaultItemData = loadResult.Value!;

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

        if (refreshUI)
            UpdateMainContent();

        return _groupData;
    }

    public void ToggleSortDirection()
    {
        SortInput = SortInput == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;

        UpdateMainContent();
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
        VaultItem? item = _vaultItemData.Items.FirstOrDefault(x => x.Id == vaultItemVM.Model.Id);
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

    private void UpdateMainContent(
        IEnumerable<VaultItem>? vaultItemData = null,
        IEnumerable<Group>? groupData = null,
        string? search = null,
        SortDirection? sortDirection = null)
    {
        vaultItemData ??= _vaultItemData.Items;
        groupData ??= _groupData.Items;
        search ??= SearchInput;
        sortDirection ??= SortInput;

        IEnumerable<VaultItemListViewModel> existingGroupedItemVMs = GroupedVaultItems.AsEnumerable();
        IEnumerable<VaultItemListViewModel> updatedGroupItemVMs = [];

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

            VaultItemListViewModel? listItemVM = group == null
                ? existingGroupedItemVMs.FirstOrDefault(x => x.Group == null) ?? new([], null)
                : existingGroupedItemVMs.FirstOrDefault(x => x.Group?.Model.Id == groupId) ?? new([], new(new GroupViewModel(group)));

            IEnumerable<VaultItemViewModelBase> itemVMs = listItemVM.VaultItems.AsEnumerable();

            foreach (VaultItem item in groupedItems)
            {
                VaultItemViewModelBase? itemVM = itemVMs.FirstOrDefault(x => x.Model.Id == item.Id);
                if (itemVM != null)
                    itemVM.Model = item;
                else
                    itemVMs = itemVMs.Append(new VaultItemViewModel(item));
            }

            IEnumerable<Guid> groupedItemIds = groupedItems.Select(x => x.Id);
            IEnumerable<VaultItemViewModelBase> itemVMsToRemove = itemVMs.Where(x => !groupedItemIds.Contains(x.Model.Id));
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
        IEnumerable<Group> groupOptions = _groupData.Items;
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