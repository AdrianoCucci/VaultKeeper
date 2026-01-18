using Microsoft.Extensions.DependencyInjection;
using System;
using VaultKeeper.AvaloniaApplication.Abstractions.ViewLocation;

namespace VaultKeeper.AvaloniaApplication.Services.ViewLocation;

public class ViewLocatorConfigOptions(IServiceCollection services)
{
    public ViewLocatorConfigOptions MapViewModelControls(Action<IViewModelControlMapper> mapHandler)
    {
        ViewModelControlMapper mapper = new(services);
        mapHandler.Invoke(mapper);

        services.AddSingleton(mapper.GetDescriptors());

        return this;
    }
}
