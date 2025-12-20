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
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class VaultPageViewModel(
    IVaultItemService vaultItemService,
    ISecurityService securityService,
    IPlatformService platformService) : ViewModelBase
{
    public ObservableCollection<VaultItemViewModel> VaultItems { get; } = [];

    [ObservableProperty]
    public bool _isReadOnly = false;

    public async Task LoadVaultItemsAsync()
    {
        await Task.Delay(1000);

        var getResult = await vaultItemService.GetManyAsync();
        //if (getResult.IsSuccessful)
        //    SetVaultItems(getResult.Value!);

        SetVaultItems(Enumerable.Range(1, 10).Select(x => new VaultItem
        {
            Name = $"{x}: My Account",
            Value = securityService.Encrypt($"{x}: Password123").Value!
        }));
    }

    public void SetVaultItems(IEnumerable<VaultItem> vaultItems)
    {
        VaultItems.Clear();

        foreach (var item in vaultItems)
        {
            VaultItems.Add(new(item));
        }
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
                itemVM.ViewMode = RecordViewMode.Edit;
                break;
            case VaultItemAction.EditCancel:
                itemVM.ViewMode = RecordViewMode.ReadOnly;
                break;
            case VaultItemAction.ToggleRevealValue:
                {
                    if (itemVM.Content is VaultItemEditViewModel editVM)
                    {
                        string? value = editVM.Form.Value;

                        if (value != null)
                        {
                            editVM.Form.Value = editVM.ValueRevealed
                                ? Encrypt(value)
                                : Decrypt(value);
                        }

                        editVM.ValueRevealed = !editVM.ValueRevealed;
                    }
                }
                break;
            case VaultItemAction.EditSave:
                {
                    if (itemVM.Content is VaultItemEditViewModel editVM)
                    {
                        string value = editVM.Form.Value ?? throw new NullReferenceException("This shouldn't happen."); // TODO: Handle properly.
                        if (editVM.ValueRevealed)
                            value = Encrypt(value);

                        itemVM.Model = editVM.Form.GetModel() with { Value = value };
                    }

                    itemVM.ViewMode = RecordViewMode.ReadOnly;
                }
                break;
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
