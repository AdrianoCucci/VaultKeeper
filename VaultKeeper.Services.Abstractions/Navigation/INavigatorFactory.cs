namespace VaultKeeper.Services.Abstractions.Navigation;

public interface INavigatorFactory
{
    INavigator? GetNavigator(string scopeKey);
    INavigator GetRequiredNavigator(string scopeKey);
}