namespace VaultKeeper.Models.ApplicationData;

public record class SavedData<T>
{
    public required SaveMetadata Metadata { get; init; }
    public required T Data { get; init; }

    public record SaveMetadata
    {
        public required int Version { get; init; }
    }
}
