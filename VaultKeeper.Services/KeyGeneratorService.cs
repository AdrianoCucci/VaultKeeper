using Microsoft.Extensions.Logging;
using System;
using System.Text;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class KeyGeneratorService(ILogger<KeyGeneratorService> logger) : IKeyGeneratorService
{
    public string GenerateKey(KeyGenerationSettings settings)
    {
        logger.LogInformation(nameof(GenerateKey));

        char[] chars = [.. settings.CharSet.Chars];
        Random random = new();
        int length = random.Next(settings.MinLength, settings.MaxLength + 1);
        StringBuilder sb = new();

        for (int i = 0; i < length; i++)
        {
            char randomChar = chars[random.Next(0, chars.Length)];
            sb.Append(randomChar);
        }

        return sb.ToString();
    }
};
