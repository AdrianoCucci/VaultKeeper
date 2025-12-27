using System;
using System.Collections.Generic;
using VaultKeeper.Models.Navigation;

namespace VaultKeeper.AvaloniaApplication.Abstractions;

public interface INavigator
{
    CurrentRoute CurrentRoute { get; }

    event EventHandler<CurrentRoute>? Navigated;

    bool Navigate(string routeKey, Dictionary<string, object?>? routeParams = null);
}