using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.Forms.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Groups;
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
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsToolbarVisible))]
    private ObservableCollection<VaultItemViewModelBase> _vaultItems = [];

    [ObservableProperty]
    private ObservableCollection<GroupViewModel> _groups = [];

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsToolbarVisible))]
    private bool _isSidePaneOpen = false;

    [ObservableProperty]
    private string? _sidePaneTitle;

    [ObservableProperty]
    private object? _sidePaneContent;

    public bool IsToolbarVisible => VaultItems.Count > 0 && !IsSidePaneOpen;

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

    public void ShowVaultItemCreateForm()
    {
        SidePaneContent = null;
        SidePaneContent = new VaultItemFormViewModel(new(), FormMode.New) { GroupOptions = Groups.Select(x => x.Model) };
        SidePaneTitle = "New Key";
        IsSidePaneOpen = true;
    }

    public void HideVaultItemCreateForm(bool clearData = false)
    {
        IsSidePaneOpen = false;

        if (clearData)
            SidePaneContent = null;
    }

    public void ShowVaultItemEditForm(VaultItemViewModel vaultItem)
    {
        var index = VaultItems.IndexOf(vaultItem);
        if (index == -1) return;

        VaultItems[index] = new VaultItemFormViewModel(vaultItem.Model, FormMode.Edit)
        {
            GroupOptions = Groups.Select(x => x.Model)
        };
    }

    public void HideVaultItemEditForm(VaultItemFormViewModel vaultItem)
    {
        var index = VaultItems.IndexOf(vaultItem);
        if (index == -1) return;

        VaultItems[index] = new VaultItemViewModel(vaultItem.Model);
    }

    public async Task DeleteVaultItemAsync(VaultItemViewModelBase vaultItem)
    {
        if (!VaultItems.Contains(vaultItem)) return;

        Result deleteResult = await vaultItemService.DeleteAsync(vaultItem.Model);
        if (!deleteResult.IsSuccessful)
            throw new Exception($"{nameof(VaultPageViewModel)}: Failed to delete item - {deleteResult.Message}", deleteResult.Exception);

        VaultItems.Remove(vaultItem);
    }

    public async Task HandleItemActionAsync(VaultItemViewModel itemVM, VaultItemAction action)
    {
        VaultItem model = itemVM.Model;

        switch (action)
        {
            case VaultItemAction.CopyName:
                await platformService.GetClipboard().SetTextAsync(model.Name);
                break;
            case VaultItemAction.CopyValue:
                await platformService.GetClipboard().SetTextAsync(Decrypt(model.Value));
                break;
            case VaultItemAction.Edit:
                ShowVaultItemEditForm(itemVM);
                break;
            case VaultItemAction.Delete:
                await DeleteVaultItemAsync(itemVM);
                break;
        }
    }

    public async Task HandleItemFormEventAsync(VaultItemFormActionEventArgs formEvent)
    {
        switch (formEvent.Action)
        {
            case VaultItemFormAction.Cancel:
                if (formEvent.ViewModel == SidePaneContent)
                    HideVaultItemCreateForm();
                else
                    HideVaultItemEditForm(formEvent.ViewModel);
                break;
            case VaultItemFormAction.Submit:
                await SaveVaultItemFormAsync(formEvent);
                break;
            case VaultItemFormAction.ToggleRevealValue:
                ToggleRevealFormItemValue(formEvent.ViewModel);
                break;
        }
    }

    public async Task SaveVaultItemFormAsync(VaultItemFormActionEventArgs formEvent)
    {
        if (formEvent.Form.HasErrors) return;

        VaultItem formModel = formEvent.Form.GetModel();
        bool shouldEncrypt = formEvent.ViewModel.ValueRevealed;

        switch (formEvent.Form.Mode)
        {
            case FormMode.New:
                {
                    Result<VaultItem> addResult = await vaultItemService.AddAsync(formModel.ToNewVaultItem(), shouldEncrypt);
                    if (!addResult.IsSuccessful)
                        throw new Exception($"{nameof(VaultPageViewModel)}: Failed to add item - {addResult.Message}", addResult.Exception);

                    HideVaultItemCreateForm();
                    await LoadVaultItemsAsync();

                    break;
                }
            case FormMode.Edit:
                {
                    Result<VaultItem> updateResult = await vaultItemService.UpdateAsync(formModel, shouldEncrypt);
                    if (!updateResult.IsSuccessful)
                        throw new Exception($"{nameof(VaultPageViewModel)}: Failed to update item - {updateResult.Message}", updateResult.Exception);

                    formEvent.ViewModel.UpdateModel(_ => updateResult.Value!);
                    HideVaultItemEditForm(formEvent.ViewModel);

                    break;
                }
        }
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