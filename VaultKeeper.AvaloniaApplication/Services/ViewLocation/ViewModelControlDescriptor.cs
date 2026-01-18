using Avalonia.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using VaultKeeper.AvaloniaApplication.Abstractions.ViewLocation;

namespace VaultKeeper.AvaloniaApplication.Services.ViewLocation;

[method: SetsRequiredMembers]
public class ViewModelControlDescriptor(Type viewModelType, Type controlType, Func<object?, Control> controlFactory) : IViewModelControlDescriptor
{
    public Type ViewModelType { get; } = viewModelType;
    public Type ControlType { get; } = controlType;

    public Control CreateControl(object? viewModel)
    {
        Control control = controlFactory.Invoke(viewModel);
        control.DataContext = viewModel;

        return control;
    }

    public Control CreateControl(Func<object?> viewModelFactory)
    {
        object? viewModel = viewModelFactory.Invoke();
        Control control = controlFactory.Invoke(viewModel);
        control.DataContext = viewModel;

        return control;
    }
}
