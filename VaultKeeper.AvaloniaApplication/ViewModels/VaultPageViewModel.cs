using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    private ObservableCollection<VaultItemViewModelBase> _vaultItems = [];

    [ObservableProperty]
    private ObservableCollection<GroupViewModel> _groups = [];

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

    public bool IsEmpty => VaultItems.Count < 1 && Groups.Count < 1;
    public bool IsToolbarVisible => !IsEmpty && !IsSidePaneOpen;

    public virtual async Task LoadDataAsync() => await Task.WhenAll(LoadVaultItemsAsync(), LoadGroupsAsync());

    public async Task LoadVaultItemsAsync()
    {
        Result<IEnumerable<VaultItem>> loadResult = await vaultItemService.GetManyAsync();
        if (!loadResult.IsSuccessful)
            throw new Exception($"{nameof(VaultPageViewModel)}: Failed to load items - {loadResult.Message}", loadResult.Exception);

        SetVaultItems(loadResult.Value!);
    }

    public async Task LoadGroupsAsync()
    {
        Result<IEnumerable<Group>> loadResult = await groupService.GetManyAsync();
        if (!loadResult.IsSuccessful)
            throw new Exception($"{nameof(VaultPageViewModel)}: Failed to load groups - {loadResult.Message}", loadResult.Exception);

        SetGroups(loadResult.Value!);
    }

    public void SetVaultItems(IEnumerable<VaultItem> vaultItems)
    {
        IEnumerable<VaultItemViewModel> viewModels = vaultItems.Select(x => new VaultItemViewModel(x));
        VaultItems = [.. viewModels];
    }

    public void SetGroups(IEnumerable<Group> groups)
    {
        IEnumerable<GroupViewModel> viewModels = groups.Select(x => new GroupViewModel(x));
        Groups = [.. viewModels];
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

    public void ShowVaultItemEditForm(VaultItemViewModelBase vaultItemVM)
    {
        TryUpdateVaultItemViewModel(vaultItemVM, () => CreateVaultItemFormViewModel(vaultItemVM.Model, FormMode.Edit));
    }

    public void HideVaultItemEditForm(VaultItemFormViewModel vaultItemVM)
    {
        TryUpdateVaultItemViewModel(vaultItemVM, () => new VaultItemViewModel(vaultItemVM.Model));
    }

    public async Task DeleteVaultItemAsync(VaultItemViewModelBase vaultItemVM)
    {
        if (!VaultItems.Any(x => x.Model.Id == vaultItemVM.Model.Id))
            return;

        Result deleteResult = await vaultItemService.DeleteAsync(vaultItemVM.Model);
        if (!deleteResult.IsSuccessful)
            throw new Exception($"{nameof(VaultPageViewModel)}: Failed to delete item - {deleteResult.Message}", deleteResult.Exception);

        if (TryUpdateVaultItemViewModel(vaultItemVM, () => null))
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
                throw new Exception($"{nameof(VaultPageViewModel)}: Failed to create group - {addGroupResult.Message}", addGroupResult.Exception);

            formModel.GroupId = addGroupResult.Value!.Id;
            await LoadGroupsAsync();
        }

        switch (formEvent.Form.Mode)
        {
            case FormMode.New:
                {
                    Result<VaultItem> addResult = await vaultItemService.AddAsync(formModel.ToNewVaultItem(), shouldEncrypt);
                    if (!addResult.IsSuccessful)
                        throw new Exception($"{nameof(VaultPageViewModel)}: Failed to create key - {addResult.Message}", addResult.Exception);

                    HideVaultItemCreateForm();

                    break;
                }
            case FormMode.Edit:
                {
                    Result<VaultItem> updateResult = await vaultItemService.UpdateAsync(formModel, shouldEncrypt);
                    if (!updateResult.IsSuccessful)
                        throw new Exception($"{nameof(VaultPageViewModel)}: Failed to update key - {updateResult.Message}", updateResult.Exception);

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

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(VaultItems) || e.PropertyName == nameof(Groups))
            UpdateMainContent();
    }

    private void UpdateMainContent()
    {
        static VaultItemListViewModel CreateListViewModel(IEnumerable<VaultItemViewModelBase> itemVMs, GroupViewModel? groupVM)
        {
            ObservableCollection<VaultItemViewModelBase> itemsCollection = [.. itemVMs];

            return new VaultItemListViewModel(itemsCollection, groupVM)
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
        }

        IEnumerable<VaultItemListViewModel> ungroupedListItems = VaultItems
            .Where(x => !x.Model.GroupId.HasValue)
            .GroupBy(x => x.Model.GroupId)
            .Select(grouping => CreateListViewModel(grouping, null));

        IEnumerable<VaultItemListViewModel> groupedListItems = Groups
            .Select(groupVM => CreateListViewModel(VaultItems.Where(x => x.Model.GroupId == groupVM.Model.Id), groupVM));

        GroupedVaultItems = [.. ungroupedListItems, .. groupedListItems];
        MainContent = GroupedVaultItems.Count > 0 ? GroupedVaultItems : EmptyViewModel.Instance;
    }

    private bool TryUpdateVaultItemViewModel(VaultItemViewModelBase vaultItemVM, Func<VaultItemViewModelBase?> newModelFunc)
    {
        VaultItemViewModelBase? existingItemVM = VaultItems.FirstOrDefault(x => x.Model.Id == vaultItemVM.Model.Id);
        if (existingItemVM == null)
            return false;

        VaultItemViewModelBase? newVM = newModelFunc.Invoke();
        if (newVM != null)
            VaultItems[VaultItems.IndexOf(existingItemVM)] = newVM;
        else
            VaultItems.Remove(existingItemVM);

        foreach (VaultItemListViewModel listGroupVM in GroupedVaultItems)
        {
            ObservableCollection<VaultItemViewModelBase> groupItemVMs = listGroupVM.VaultItems;
            var existingGroupedItemVM = groupItemVMs.FirstOrDefault(x => x.Model.Id == vaultItemVM.Model.Id);

            if (existingGroupedItemVM != null)
            {
                if (newVM != null)
                    groupItemVMs[groupItemVMs.IndexOf(existingGroupedItemVM)] = newVM;
                else
                    groupItemVMs.Remove(existingGroupedItemVM);

                break;
            }
        }

        return true;
    }

    private VaultItemFormViewModel CreateVaultItemFormViewModel(VaultItem vaultItem, FormMode formMode)
    {
        IEnumerable<Group> groupOptions = Groups.Select(x => x.Model);
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