using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Rescale.Models;
using Rescale.Services;

namespace Rescale.ViewModels;

/// <summary>Top-level view model owning the preset list, detail, and settings sub-viewmodels.</summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private readonly PresetService _presetService;
    private readonly DisplayService _displayService;

    [ObservableProperty]
    private AppConfig _config = new();

    [ObservableProperty]
    private PresetListViewModel _presetListVm;

    [ObservableProperty]
    private PresetDetailViewModel? _presetDetailVm;

    [ObservableProperty]
    private SettingsViewModel _settingsVm;

    [ObservableProperty]
    private string _currentPage = "presets";

    [ObservableProperty]
    private bool _isDetailView;

    public MainViewModel(
        ConfigService configService,
        PresetService presetService,
        DisplayService displayService,
        ResolutionService resolutionService,
        HdrService hdrService)
    {
        _configService = configService;
        _presetService = presetService;
        _displayService = displayService;

        Config = configService.Load();

        _presetListVm = new PresetListViewModel(Config, presetService, displayService, configService);
        _presetListVm.PresetSelected += OnPresetSelected;
        _presetListVm.PresetCreated += OnPresetCreated;

        _settingsVm = new SettingsViewModel(Config, configService);
    }

    private void OnPresetSelected(Preset preset)
    {
        PresetDetailVm = new PresetDetailViewModel(
            preset, Config, _presetService, _displayService, _configService);
        PresetDetailVm.BackRequested += OnDetailBack;
        IsDetailView = true;
    }

    private void OnPresetCreated(Preset preset)
    {
        OnPresetSelected(preset);
    }

    private void OnDetailBack()
    {
        IsDetailView = false;
        PresetDetailVm = null;
        PresetListVm.Refresh();
    }

    [RelayCommand]
    private void NavigateTo(string page)
    {
        CurrentPage = page;
        IsDetailView = false;
        PresetDetailVm = null;
    }
}
