using Avalonia;
using Microsoft.Extensions.Logging;
using System;
using VaultKeeper.AvaloniaApplication.Abstractions;

namespace VaultKeeper.AvaloniaApplication.Services;

public class ApplicationService(ILogger<ApplicationService> logger) : IApplicationService
{
    public Application GetApplication()
    {
        logger.LogInformation(nameof(GetApplication));

        Application? application = Application.Current;
        if (application == null)
        {
            InvalidOperationException ex = new($"Current Application state is not defined.");
            logger.LogError($"{nameof(ApplicationService)} - {{error}}", ex.Message);

            throw ex;
        }

        return application;
    }
}
