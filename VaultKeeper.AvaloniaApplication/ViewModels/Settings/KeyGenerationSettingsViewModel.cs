using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Settings;

public partial class KeyGenerationSettingsViewModel(KeyGenerationSettings model, IEnumerable<CharSet>? charSetOptions = null) : ViewModelBase<KeyGenerationSettings>(model)
{
    [ObservableProperty]
    private CharSet _charSet = charSetOptions?.FirstOrDefault(x => x.Type == model.CharSet.Type) ?? model.CharSet;

    [ObservableProperty]
    private IEnumerable<CharSet> _charSetOptions = charSetOptions ?? [];

    private int _minLength = Math.Clamp(model.MinLength, 1, model.MaxLength);
    public int MinLength { get => _minLength; set => SetProperty(ref _minLength, Math.Clamp(value, 1, _maxLength)); }

    private int _maxLength = model.MaxLength;
    public int MaxLength
    {
        get => _maxLength;
        set
        {
            SetProperty(ref _maxLength, value);
            if (value < _minLength)
                MinLength = value;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ViewMaxWidth), nameof(ViewMaxHeight), nameof(ScrollViewerMaxHeight), nameof(ListBoxItemMargin))]
    private bool _useCompactView = false;

    public double ViewMaxWidth => UseCompactView ? 800 : double.NaN;
    public double ViewMaxHeight => UseCompactView ? 600 : double.NaN;
    public double ScrollViewerMaxHeight => UseCompactView ? 300 : double.NaN;
    public Thickness ListBoxItemMargin => UseCompactView ? new(0) : new(10);

    private readonly ICharSetService? _charSetService;
    private readonly IUserSettingsService? _userSettingsService;

    public KeyGenerationSettingsViewModel() : this(KeyGenerationSettings.Default) { }

    public KeyGenerationSettingsViewModel(ICharSetService? charSetService, IUserSettingsService? userSettingsService) : this()
    {
        _charSetService = charSetService;
        _userSettingsService = userSettingsService;

        ApplyDefaultSettings();
    }

    public void ApplySettings(KeyGenerationSettings settings)
    {
        Model = settings;
        MaxLength = settings.MaxLength;
        MinLength = settings.MinLength;

        if (_charSetService != null)
        {
            CharSetOptions = _charSetService.GetCharSets();

            CharSet? currentCharSet = CharSetOptions.FirstOrDefault(x => x.Type == settings.CharSet.Type);
            CharSet = currentCharSet ?? CharSetOptions.FirstOrDefault() ?? settings.CharSet;
        }
        else
        {
            CharSet = settings.CharSet;
        }
    }

    public void ApplyDefaultSettings()
    {
        KeyGenerationSettings defaultSettings = _userSettingsService?.GetUserSettingsOrDefault()?.KeyGeneration ?? KeyGenerationSettings.Default;
        ApplySettings(defaultSettings);
    }

    public override KeyGenerationSettings GetUpdatedModel() => new()
    {
        CharSet = CharSet,
        MinLength = MinLength,
        MaxLength = MaxLength
    };
}
