namespace VaultKeeper.Common.Logging;

public enum LogLevel
{
    Verbose,
    Debug,
    Information,
    Warning,
    Error,
    Fatal
}

public enum FileLoggingRollingInterval
{
    Infinite,
    Year,
    Month,
    Day,
    Hour,
    Minute
}

public record LoggingConfig
{
    public required LogLevel LogLevel { get; init; }
    public FileLoggingConfig? FileLoggingConfig { get; init; }
}

public record FileLoggingConfig
{
    public required string FilePath { get; init; }
    public int? FileLimit { get; init; }
    public FileLoggingRollingInterval RollingInterval { get; init; }
}
