using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Forms;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Models.VaultItems.Extensions;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class VaultPageViewModel(
    IVaultItemService vaultItemService,
    ISecurityService securityService,
    IPlatformService platformService) : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<VaultItemViewModelBase> _vaultItems = [];

    [ObservableProperty]
    private VaultItemFormViewModel? _newVaultItemForm;

    [ObservableProperty]
    public bool _isSidePaneOpen = false;

    private bool _isLoading;
    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }

    private bool _hasVaultItems;
    public bool HasVaultItems { get => _hasVaultItems; private set => SetProperty(ref _hasVaultItems, value); }

    public async Task LoadVaultItemsAsync()
    {
        IsLoading = true;

        var loadResult = await vaultItemService.LoadAllAsync();
        if (!loadResult.IsSuccessful)
        {
            // TODO: Handle error.
        }

        SetVaultItems(loadResult.Value!);
        IsLoading = false;
    }

    public void SetVaultItems(IEnumerable<VaultItem> vaultItems)
    {
        IEnumerable<VaultItemViewModel> viewModels = vaultItems.Select(x => new VaultItemViewModel(x));
        VaultItems = [.. viewModels];
        HasVaultItems = VaultItems.Count > 0;
    }

    public void ShowVaultItemCreateForm()
    {
        NewVaultItemForm = null;
        NewVaultItemForm = new(new(), FormMode.New);
        IsSidePaneOpen = true;
    }

    public void HideVaultItemCreateForm(bool clearData = false)
    {
        IsSidePaneOpen = false;

        if (clearData)
            NewVaultItemForm = null;
    }

    public void ShowVaultItemEditForm(VaultItemViewModel vaultItem)
    {
        var index = VaultItems.IndexOf(vaultItem);
        if (index == -1) return;

        VaultItems[index] = new VaultItemFormViewModel(vaultItem.Model, FormMode.Edit);
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
        {
            // TODO: Handle error
            return;
        }

        VaultItems.Remove(vaultItem);
        HasVaultItems = VaultItems.Count > 0;
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
                if (formEvent.ViewModel == NewVaultItemForm)
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
                    {
                        // TODO: handle error.
                        break;
                    }

                    HideVaultItemCreateForm();
                    await LoadVaultItemsAsync();

                    break;
                }
            case FormMode.Edit:
                {
                    Result<VaultItem> updateResult = await vaultItemService.UpdateAsync(formModel, shouldEncrypt);
                    if (!updateResult.IsSuccessful)
                    {
                        // TODO: handle error.
                        break;
                    }

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