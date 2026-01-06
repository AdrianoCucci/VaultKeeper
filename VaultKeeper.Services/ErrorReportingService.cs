using Microsoft.Extensions.Logging;
using System;
using VaultKeeper.Models.Errors;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class ErrorReportingService(ILogger<ErrorReportingService> logger) : IErrorReportingService
{
    public event EventHandler<Error>? ErrorReported;

    public void ReportError(Error error)
    {
        logger.LogInformation($"{nameof(ReportError)} | Error: {{error}}", error);
        ErrorReported?.Invoke(this, error);
    }
}
