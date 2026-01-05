using System;

namespace VaultKeeper.Models.Errors;

public record Error
{
    public required string Header { get; set; }
    public required string Message { get; set; }
    public ErrorSeverity Severity { get; set; } = ErrorSeverity.Normal;
    public ErrorSource Source { get; set; } = ErrorSource.Application;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Exception? Exception { get; set; }
}
