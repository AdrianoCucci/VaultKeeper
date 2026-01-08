using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VaultKeeper.Models.Groups;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Groups;

public partial class GroupSelectInputViewModel : ViewModelBase
{
    [ObservableProperty]
    private IEnumerable<Group> _groupOptions;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(WillCreateGroup))]
    private Group? _selectedGroup;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(WillCreateGroup))]
    private string? _inputText;

    public bool WillCreateGroup => (SelectedGroup == null || SelectedGroup.Id == Guid.Empty) && !string.IsNullOrWhiteSpace(InputText);

    public GroupSelectInputViewModel(IEnumerable<Group>? groupOptions = null, Group? selectedGroup = null)
    {
        _groupOptions = groupOptions ?? [];
        _selectedGroup = groupOptions?.FirstOrDefault(x => x.Id == selectedGroup?.Id);
        _inputText = _selectedGroup?.Name;
    }

    public GroupSelectInputViewModel() : this(null, null) { }

    public Group GetGroup() => SelectedGroup ?? new()
    {
        Id = Guid.Empty,
        Name = InputText ?? string.Empty,
    };

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (SelectedGroup == null) return;

        switch (e.PropertyName)
        {
            case nameof(SelectedGroup):
                InputText = SelectedGroup.Name;
                break;
            case nameof(GroupOptions):
                if (!GroupOptions.Contains(SelectedGroup))
                {
                    SelectedGroup = null;
                    InputText = null;
                }
                break;
        }
    }
}
