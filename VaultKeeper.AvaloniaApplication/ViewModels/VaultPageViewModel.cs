using CommunityToolkit.Mvvm.ComponentModel;
using System;
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
    private ObservableCollection<VaultItemViewModel> _vaultItems = [];

    [ObservableProperty]
    private VaultItemViewModel? _focusedVaultItemVM;

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

    public void ShowVaultItemForm(VaultItemViewModel? itemVM = null)
    {
        if (itemVM != null)
        {
            itemVM.ViewMode = RecordViewMode.Edit;
            FocusedVaultItemVM = itemVM.Content as VaultItemViewModel;
        }
        else
        {
            FocusedVaultItemVM = new(new() { Id = Guid.NewGuid() }) { ViewMode = RecordViewMode.Edit };
        }

        IsSidePaneOpen = true;
    }

    public void HideVaultItemForm(VaultItemFormViewModel? formVM = null)
    {
        formVM ??= FocusedVaultItemVM?.Content as VaultItemFormViewModel;

        if (formVM != null)
            formVM.ViewMode = RecordViewMode.Default;

        IsSidePaneOpen = false;
    }

    public async Task HandleFormEventAsync(VaultItemFormActionEventArgs formEvent)
    {
        switch (formEvent.Action)
        {
            case VaultItemFormAction.Cancel:
                HideVaultItemForm(formEvent.ViewModel);
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

        Result<VaultItem> upsertResult = formEvent.Form.Mode switch
        {
            FormMode.New => await vaultItemService.AddAsync(formModel.ToNewVaultItem(), shouldEncrypt),
            FormMode.Edit => await vaultItemService.UpdateAsync(formModel, shouldEncrypt),
            _ => throw new ArgumentOutOfRangeException(nameof(formEvent))
        };

        if (upsertResult.IsSuccessful)
        {
            formEvent.ViewModel.UpdateModel(_ => upsertResult.Value!);
            HideVaultItemForm(formEvent.ViewModel);

            if (formEvent.Form.Mode == FormMode.New)
            {
                await LoadVaultItemsAsync();
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
                {
                    if (itemVM is VaultItemViewModel listVM)
                        listVM.ViewMode = RecordViewMode.Edit;
                    break;
                }
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