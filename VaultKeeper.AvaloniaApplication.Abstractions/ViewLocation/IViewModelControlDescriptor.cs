using Avalonia.Controls;
using System;

namespace VaultKeeper.AvaloniaApplication.Abstractions.ViewLocation;

public interface IViewModelControlDescriptor
{
    Type ViewModelType { get; }
    Type ControlType { get; }

    Control CreateControl();
    Control CreateControl(Func<object?> viewModelFactory);
    Control CreateControl(object? viewModel);
}