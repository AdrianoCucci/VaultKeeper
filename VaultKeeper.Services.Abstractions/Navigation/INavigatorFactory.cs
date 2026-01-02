using System.Collections.Generic;

namespace VaultKeeper.Services.Abstractions.Navigation;

public interface INavigatorFactory
{
    INavigator? GetNavigator(string scopeKey);
    INavigator GetRequiredNavigator(string scopeKey);
    IEnumerable<INavigator> GetAllNavigators();
}