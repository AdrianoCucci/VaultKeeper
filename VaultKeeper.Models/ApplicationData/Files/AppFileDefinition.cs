namespace VaultKeeper.Models.ApplicationData.Files;

public record AppFileDefinition
{
    public AppFileType FileType { get; init; }
    public required string Name { get; init; }
    public required string Extension { get; init; }

    public string FullName => $"{Name}{Extension}";
}
