using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VaultKeeper.AvaloniaApplication.Abstractions.ViewLocation;

namespace VaultKeeper.AvaloniaApplication.Services.ViewLocation;

public class ViewLocatorService(
    Dictionary<Type, IViewModelControlDescriptor> viewModelControlDescriptorMap,
    IServiceProvider serviceProvider) : IViewLocatorService
{
    private readonly ILogger<ViewLocatorService>? _logger = serviceProvider.GetService<ILogger<ViewLocatorService>>();

    public TView? GetView<TViewModel, TView>(TViewModel? viewModel = null) where TViewModel : class where TView : Control
    {
        Type viewModelType = viewModel?.GetType() ?? typeof(TViewModel);
        _logger?.LogInformation($"{nameof(GetView)} | Type: {{type}}", viewModelType);

        if (viewModelControlDescriptorMap.TryGetValue(viewModelType, out IViewModelControlDescriptor? descriptor) && descriptor.ControlType == typeof(TView))
            return (TView)descriptor.CreateControl(viewModel);

        return null;
    }

    public TView GetRequiredView<TViewModel, TView>(TViewModel? viewModel = null) where TViewModel : class where TView : Control
    {
        Type viewModelType = viewModel?.GetType() ?? typeof(TViewModel);
        _logger?.LogInformation($"{nameof(GetRequiredView)} | Type: {{type}}", viewModelType);

        if (viewModelControlDescriptorMap.TryGetValue(viewModelType, out IViewModelControlDescriptor? descriptor) && descriptor.ControlType == typeof(TView))
            return (TView)descriptor.CreateControl(viewModel);

        throw ViewNotRegisteredException(viewModelType);
    }

    public Control? GetView<TViewModel>(TViewModel? viewModel = null) where TViewModel : class
    {
        Type viewModelType = viewModel?.GetType() ?? typeof(TViewModel);
        _logger?.LogInformation($"{nameof(GetView)} | Type: {{type}}", viewModelType);

        if (viewModelControlDescriptorMap.TryGetValue(viewModelType, out IViewModelControlDescriptor? descriptor))
            return descriptor.CreateControl(viewModel);

        return null;
    }

    public Control GetRequiredView<TViewModel>(TViewModel? viewModel = null) where TViewModel : class
    {
        Type viewModelType = viewModel?.GetType() ?? typeof(TViewModel);
        _logger?.LogInformation($"{nameof(GetRequiredView)} | Type: {{type}}", viewModelType);

        if (viewModelControlDescriptorMap.TryGetValue(viewModelType, out IViewModelControlDescriptor? descriptor))
            return descriptor.CreateControl(viewModel);

        throw ViewNotRegisteredException(viewModelType);
    }

    private ArgumentOutOfRangeException ViewNotRegisteredException(Type viewModelType)
    {
        ArgumentOutOfRangeException exception = new(nameof(viewModelType), viewModelType, $"{nameof(ViewLocator)} has no registered {nameof(Control)} for requested ViewModel type: {viewModelType}");
        _logger?.LogError(exception, $"{nameof(ViewLocator)} Error: {{message}}", exception.Message);

        throw exception;
    }
}
