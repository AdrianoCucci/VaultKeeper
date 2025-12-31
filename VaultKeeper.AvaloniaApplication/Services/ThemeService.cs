using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Abstractions.Models;
using VaultKeeper.AvaloniaApplication.Extensions;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.AvaloniaApplication.Services;

public class ThemeService(ILogger<ThemeService> logger, IApplicationService applicationService) : IThemeService
{
    private readonly Lazy<IEnumerable<AppThemeDefinition>> _appThemeDefinitionsLazy = new(() =>
    {
        Application application = applicationService.GetApplication();

        return
        [
            new()
            {
                ThemeType = AppThemeType.System,
                BackgroundBrush = new SolidColorBrush(application.GetResourceOrDefault<Color>("SolidBackgroundFillColorBase")),
                ForegroundBrush = new SolidColorBrush(application.GetResourceOrDefault<Color>("SystemAccentColor")),
            },
            new()
            {
                ThemeType = AppThemeType.Light,
                BackgroundBrush = new SolidColorBrush(Colors.WhiteSmoke),
                ForegroundBrush = new SolidColorBrush(application.GetResourceOrDefault<Color>("SystemAccentColorLight1")),
            },
            new()
            {
                ThemeType = AppThemeType.Dark,
                BackgroundBrush = new SolidColorBrush(Colors.DarkGray),
                ForegroundBrush = new SolidColorBrush(application.GetResourceOrDefault<Color>("SystemAccentColorDark1")),
            },
            new()
            {
                ThemeType = AppThemeType.HighContrast,
                BackgroundBrush = new SolidColorBrush(Colors.Black),
                ForegroundBrush = new SolidColorBrush(Colors.White)
            }
        ];
    });

    public IEnumerable<AppThemeDefinition> GetThemeDefinitions()
    {
        logger.LogInformation(nameof(GetThemeDefinitions));
        return _appThemeDefinitionsLazy.Value;
    }

    public AppThemeDefinition GetThemeDefinitionByType(AppThemeType themeType)
    {
        logger.LogInformation(nameof(GetThemeDefinitionByType));
        return _appThemeDefinitionsLazy.Value.First(x => x.ThemeType == themeType);
    }

    public void SetTheme(AppThemeType themeType)
    {
        logger.LogInformation($"{nameof(SetTheme)} | theme: {{theme}}", themeType);
        Application application = applicationService.GetApplication();

        ThemeVariant themeVariant = themeType switch
        {
            AppThemeType.Dark => ThemeVariant.Dark,
            AppThemeType.Light => ThemeVariant.Light,
            AppThemeType.HighContrast => new("HighContrast", ThemeVariant.Light),
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
