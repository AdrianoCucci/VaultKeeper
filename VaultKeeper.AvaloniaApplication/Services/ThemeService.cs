using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Abstractions.Models;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.AvaloniaApplication.Services;

public class ThemeService(ILogger<ThemeService> logger, IApplicationService applicationService) : IThemeService
{
    private readonly Lazy<IEnumerable<AppThemeDefinition>> _themeDefinitionsLazy = new(() =>
    {
        Application application = applicationService.GetApplication();

        IDictionary<ThemeVariant, ColorPaletteResources>? fluentThemePalettes = application.Styles.OfType<FluentTheme>().FirstOrDefault()?.Palettes;
        if (fluentThemePalettes == null)
            return [];

        ColorPaletteResources systemColors = fluentThemePalettes[application.ActualThemeVariant];
        ColorPaletteResources lightColors = fluentThemePalettes[ThemeVariant.Light];
        ColorPaletteResources darkColors = fluentThemePalettes[ThemeVariant.Dark];

        return
        [
            new()
            {
                ThemeType = AppThemeType.System,
                ThemeName = "System",
                BackgroundBrush = new SolidColorBrush(systemColors.BaseLow),
                ForegroundBrush = new SolidColorBrush(systemColors.Accent),
            },
            new()
            {
                ThemeType = AppThemeType.Light,
                ThemeName = "Light",
                BackgroundBrush = new SolidColorBrush(lightColors.BaseLow),
                ForegroundBrush = new SolidColorBrush(lightColors.Accent),
            },
            new()
            {
                ThemeType = AppThemeType.Dark,
                ThemeName = "Dark",
                BackgroundBrush = new SolidColorBrush(darkColors.BaseLow),
                ForegroundBrush = new SolidColorBrush(darkColors.Accent),
            },
        ];
    });

    public IEnumerable<AppThemeDefinition> GetThemeDefinitions()
    {
        logger.LogInformation(nameof(GetThemeDefinitions));
        return _themeDefinitionsLazy.Value;
    }

    public AppThemeDefinition GetThemeDefinitionByType(AppThemeType themeType)
    {
        logger.LogInformation(nameof(GetThemeDefinitionByType));
        return _themeDefinitionsLazy.Value.First(x => x.ThemeType == themeType);
    }

    public void SetTheme(AppThemeType themeType)
    {
        logger.LogInformation($"{nameof(SetTheme)} | theme: {{theme}}", themeType);
        Application application = applicationService.GetApplication();

        ThemeVariant themeVariant = themeType switch
        {
            AppThemeType.Dark => ThemeVariant.Dark,
            AppThemeType.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };

        application.RequestedThemeVariant = themeVariant;
    }

    public void SetBaseFontSize(double fontSize)
    {
        logger.LogInformation($"{nameof(SetBaseFontSize)} | size: {{fontSize}}", fontSize);

        Application application = applicationService.GetApplication();
        application.Resources["ControlContentThemeFontSize"] = fontSize;
    }
}
