using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace VaultKeeper.AvaloniaApplication.Abstractions.ViewLocation;

public interface IViewModelControlMapper
{
    IViewModelControlMapper Map<TViewModel, TControl>() where TViewModel : class where TControl : Control, new();

    Dictionary<Type, IViewModelControlDescriptor> GetDescriptors();
}