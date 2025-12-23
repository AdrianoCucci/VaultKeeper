using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
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

    public async Task LoadVaultItemsAsync()
    {
        var loadResult = await vaultItemService.LoadAllAsync();
        if (loadResult.IsSuccessful)
            SetVaultItems(loadResult.Value!);
    }

    public void SetVaultItems(IEnumerable<VaultItem> vaultItems)
    {
        var viewModels = vaultItems.Select(x => new VaultItemViewModel(x));
        VaultItems = [.. viewModels];
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

    public async Task HandleFormEventAsync(VaultItemFormActionEventArgs formEvent)
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

    public async Task HandleVaultItemEventActionAsync(VaultItemViewModel itemVM, VaultItemAction action)
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
                //case VaultItemAction.EditCancel:
                //    {
                //        if (itemVM is VaultItemViewModel listVM)
                //            listVM.ViewMode = RecordViewMode.ReadOnly;

                //        HideVaultItemForm();
                //        break;
                //    }
                //case VaultItemAction.ToggleRevealValue:
                //    {
                //        VaultItemFormViewModel? formVM = itemVM switch
                //        {
                //            VaultItemFormViewModel vm => vm,
                //            VaultItemViewModel vm => vm.Content is VaultItemFormViewModel fvm ? fvm : null,
                //            _ => null
                //        };

                //        if (formVM == null) break;

                //        string? value = formVM.Form.Value;

                //        if (value != null)
                //        {
                //            formVM.Form.Value = formVM.ValueRevealed
                //                ? Encrypt(value)
                //                : Decrypt(value);
                //        }

                //        formVM.ValueRevealed = !formVM.ValueRevealed;

                //        break;
                //    }
                //case VaultItemAction.EditSave:
                //    {
                //        VaultItemFormViewModel? formVM = itemVM switch
                //        {
                //            VaultItemFormViewModel vm => vm,
                //            VaultItemViewModel vm => vm.Content is VaultItemFormViewModel fvm ? fvm : null,
                //            _ => null
                //        };

                //        if (formVM == null) break;

                //        string value = formVM.Form.Value ?? throw new NullReferenceException("This shouldn't happen."); // TODO: Handle properly.
                //        if (formVM.ValueRevealed)
                //            value = Encrypt(value);

                //        var updateModel = formVM.Form.GetModel() with { Value = value };

                //        var updateResult = await vaultItemService.UpdateAsync(updateModel);
                //        if (updateResult.IsSuccessful)
                //        {
                //            itemVM.Model = updateResult.Value!;

                //            if (itemVM is VaultItemViewModel listVM)
                //            {
                //                listVM.ViewMode = RecordViewMode.ReadOnly;
                //                HideVaultItemForm();
                //            }
                //        }

                //        break;
                //    }
        }
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