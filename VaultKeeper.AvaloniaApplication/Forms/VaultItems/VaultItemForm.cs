using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

public partial class VaultItemForm(VaultItem vaultItem, FormMode mode = FormMode.New, Group? group = null) : Form<VaultItem>(mode)
{
    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is required.")]
    private string? _name = vaultItem.Name;

    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Value is required.")]
    private string? _value = vaultItem.Value;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(WillCreateGroup))]
    private string? _groupName = group?.Name;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(WillCreateGroup))]
    private Group? _selectedGroup = group;

    [ObservableProperty]
    private IEnumerable<Group> _groupOptions = [];

    public VaultItem OriginalModel => vaultItem;
    public bool WillCreateGroup => (SelectedGroup == null || SelectedGroup.Id == Guid.Empty) && !string.IsNullOrWhiteSpace(GroupName);

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (SelectedGroup == null) return;

        switch (e.PropertyName)
        {
            case nameof(SelectedGroup):
                GroupName = SelectedGroup.Name;
                break;
            case nameof(GroupOptions):
                if (!GroupOptions.Contains(SelectedGroup))
                    SelectedGroup = null;
                break;
        }
    }

    public override VaultItem GetModel() => vaultItem with
    {
        Name = Name ?? string.Empty,
        Value = Value ?? string.Empty,
        GroupId = SelectedGroup?.Id
    };

    public Group GetGroup() => SelectedGroup ?? new()
    {
        Id = Guid.Empty,
        Name = GroupName ?? string.Empty
    };
}