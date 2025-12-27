using System;
using System.Collections.Generic;
using VaultKeeper.Models.Navigation;

namespace VaultKeeper.AvaloniaApplication.Abstractions.Navigation;

public interface INavigator
{
    event EventHandler<CurrentRoute>? Navigated;
    CurrentRoute CurrentRoute { get; }
    string ScopeKey { get; }


    CurrentRoute Navigate(string routeKey, Dictionary<string, object?>? routeParams = null);
}