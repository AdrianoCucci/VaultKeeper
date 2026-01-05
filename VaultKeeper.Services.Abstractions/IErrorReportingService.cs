using System;
using VaultKeeper.Models.Errors;

namespace VaultKeeper.Services.Abstractions;

public interface IErrorReportingService
{
    event EventHandler<Error>? ErrorReported;

    void ReportError(Error error);
}