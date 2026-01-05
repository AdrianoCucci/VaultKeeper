using System;
using VaultKeeper.Models.Errors;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class ErrorReportingService : IErrorReportingService
{
    public event EventHandler<Error>? ErrorReported;

    public void ReportError(Error error) => ErrorReported?.Invoke(this, error);
}
