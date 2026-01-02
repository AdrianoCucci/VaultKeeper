using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Settings;

public partial class CharSetViewModel(CharSet model) : ViewModelBase<CharSet>(model)
{
    [ObservableProperty]
    private CharSetType _type = model.Type;

    [ObservableProperty]
    private string _name = model.Name;

    [ObservableProperty]
    private IEnumerable<char> _chars = model.Chars;

    public CharSetViewModel() : this(CharSet.Default) { }
}
