using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using VaultKeeper.AvaloniaApplication.Abstractions.ViewLocation;

namespace VaultKeeper.AvaloniaApplication.Services.ViewLocation;

public class ViewModelControlMapper(IServiceCollection serviceCollection) : IViewModelControlMapper
{
    private readonly Dictionary<Type, IViewModelControlDescriptor> _descriptors = [];

    public IViewModelControlMapper Map<TViewModel, TControl>() where TViewModel : class where TControl : Control, new()
    {
        Type viewModelType = typeof(TViewModel);

        ViewModelControlDescriptor descriptor = new(
            viewModelType,
            typeof(TControl),
            viewModel => new TControl { DataContext = viewModel });

        serviceCollection.AddTransient<TViewModel>();
        serviceCollection.AddSingleton(descriptor);
        _descriptors[viewModelType] = descriptor;

        return this;
    }

    public Dictionary<Type, IViewModelControlDescriptor> GetDescriptors() => _descriptors;
}