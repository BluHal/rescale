using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Rescale.Models;
using Rescale.Services;

namespace Rescale.ViewModels;

/// <summary>View model for the preset list page, managing favorites and library collections.</summary>
public partial class PresetListViewModel : ObservableObject
{
    private readonly AppConfig _config;
    private readonly PresetService _presetService;
    private readonly DisplayService _displayService;
    private readonly ConfigService _configService;

    /// <summary>Raised when the user selects a preset to edit.</summary>
    public event Action<Preset>? PresetSelected;

    /// <summary>Raised when a new preset is created and should be opened for editing.</summary>
    public event Action<Preset>? PresetCreated;

    [ObservableProperty]
    private ObservableCollection<Preset> _favorites = [];

    [ObservableProperty]
    private ObservableCollection<Preset> _library = [];

    [ObservableProperty]
    private string? _activePresetId;

    public PresetListViewModel(
        AppConfig config,
        PresetService presetService,
        DisplayService displayService,
        ConfigService configService)
    {
        _config = config;
        _presetService = presetService;
        _displayService = displayService;
        _configService = configService;
        Refresh();
    }

    /// <summary>Reloads the favorites and library collections from persisted config.</summary>
    public void Refresh()
    {
        var config = _configService.Load();
        ActivePresetId = config.ActivePresetId;

        Favorites = new ObservableCollection<Preset>(
            config.Presets.Where(p => p.IsFavorite).OrderBy(p => p.Order));

        Library = new ObservableCollection<Preset>(
            config.Presets.Where(p => !p.IsFavorite).OrderBy(p => p.Name));
    }

    [RelayCommand]
    private void OpenPreset(Preset preset)
    {
        Services.LogService.Info($"OpenPreset command: {preset.Name} (Id={preset.Id})");
        PresetSelected?.Invoke(preset);
    }

    [RelayCommand]
    private void CaptureNewPreset()
    {
        try
        {
            Services.LogService.Info("CaptureNewPreset");
            var preset = _presetService.CaptureCurrentAsPreset("New preset");
            Refresh();
            PresetCreated?.Invoke(preset);
        }
        catch (Exception ex)
        {
            Services.LogService.Error("CaptureNewPreset failed", ex);
        }
    }

    [RelayCommand]
    private void CreateEmptyPreset()
    {
        try
        {
            Services.LogService.Info("CreateEmptyPreset");
            var config = _configService.Load();
            var preset = new Preset
            {
                Name = "New preset",
                Order = config.Presets.Count(p => p.IsFavorite),
            };
            config.Presets.Add(preset);
            _configService.Save(config);
            Refresh();
            Services.LogService.Info($"Empty preset created: {preset.Id}");
            PresetCreated?.Invoke(preset);
        }
        catch (Exception ex)
        {
            Services.LogService.Error("CreateEmptyPreset failed", ex);
        }
    }
}
