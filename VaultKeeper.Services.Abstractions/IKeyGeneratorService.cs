using VaultKeeper.Models.Settings;

namespace VaultKeeper.Services.Abstractions;

public interface IKeyGeneratorService
{
    string GenerateKey(KeyGenerationSettings settings);
}