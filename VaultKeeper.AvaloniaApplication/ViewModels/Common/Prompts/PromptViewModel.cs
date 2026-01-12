using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

public partial class PromptViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _header;

    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private object? _content;

    [ObservableProperty]
    private bool _showOkButton = true;

    [ObservableProperty]
    private double _contentMinWidth = double.NaN;

    [ObservableProperty]
    private double _contentMaxWidth = double.NaN;

    [ObservableProperty]
    private double _contentMinHeight = double.NaN;

    [ObservableProperty]
    private double _contentMaxHeight = double.NaN;

    public Action? AckwnoledgedAction { get; set; }

    public void Acknowledge() => AckwnoledgedAction?.Invoke();
}
