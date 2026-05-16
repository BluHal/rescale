using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Rescale.Models;
using Rescale.Services;

namespace Rescale.ViewModels;

/// <summary>View model for the preset detail/editor page with per-monitor configuration.</summary>
public partial class PresetDetailViewModel : ObservableObject
{
    private readonly PresetService _presetService;
    private readonly DisplayService _displayService;
    private readonly ConfigService _configService;
    private readonly AppConfig _appConfig;

    /// <summary>Raised when the user navigates back to the preset list.</summary>
    public event Action? BackRequested;

    [ObservableProperty]
    private Preset _preset;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _icon;

    [ObservableProperty]
    private bool _isFavorite;

    [ObservableProperty]
    private string? _hotkey;

    [ObservableProperty]
    private ObservableCollection<MonitorConfig> _monitors;

    [ObservableProperty]
    private int _selectedMonitorIndex;

    [ObservableProperty]
    private MonitorConfig? _selectedMonitor;

    [ObservableProperty]
    private ObservableCollection<DisplayMode> _availableResolutions = [];

    [ObservableProperty]
    private ObservableCollection<int> _availableRefreshRates = [];

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private bool _isIconPickerOpen;

    [ObservableProperty]
    private HashSet<string> _connectedDevicePaths = [];

    public PresetDetailViewModel(
        Preset preset,
        AppConfig appConfig,
        PresetService presetService,
        DisplayService displayService,
        ConfigService configService)
    {
        _preset = preset;
        _appConfig = appConfig;
        _presetService = presetService;
        _displayService = displayService;
        _configService = configService;

        _name = preset.Name;
        _icon = preset.Icon;
        _isFavorite = preset.IsFavorite;
        _hotkey = preset.Hotkey;
        _monitors = new ObservableCollection<MonitorConfig>(preset.Monitors);
        _isActive = appConfig.ActivePresetId == preset.Id;
        RefreshConnectedMonitors();

        if (_monitors.Count > 0)
        {
            SelectedMonitorIndex = 0;
            SelectedMonitor = _monitors[0];
        }
    }

    partial void OnSelectedMonitorIndexChanged(int value)
    {
        if (value >= 0 && value < Monitors.Count)
        {
            SelectedMonitor = Monitors[value];
            LoadAvailableModesForMonitor();
        }
    }

    private void LoadAvailableModesForMonitor()
    {
        if (SelectedMonitor == null) return;

        var activeMonitors = _displayService.GetActiveMonitors();
        var match = activeMonitors.FirstOrDefault(m =>
            string.Equals(m.DevicePath, SelectedMonitor.DevicePath, StringComparison.OrdinalIgnoreCase));

        if (match == null) return;

        var modes = _displayService.GetSupportedModes(match.GdiDeviceName);
        AvailableResolutions = new ObservableCollection<DisplayMode>(
            modes.DistinctBy(m => (m.Width, m.Height)).ToList());

        var refreshRates = modes
            .Where(m => m.Width == SelectedMonitor.Width && m.Height == SelectedMonitor.Height)
            .Select(m => m.RefreshRate)
            .Distinct()
            .OrderByDescending(r => r)
            .ToList();
        AvailableRefreshRates = new ObservableCollection<int>(refreshRates);
    }

    private void RefreshConnectedMonitors()
    {
        ConnectedDevicePaths = new HashSet<string>(
            _displayService.GetActiveMonitors().Select(m => m.DevicePath),
            StringComparer.OrdinalIgnoreCase);
    }

    [RelayCommand]
    private void GoBack() => BackRequested?.Invoke();

    [RelayCommand]
    private void Save()
    {
        Preset.Name = Name;
        Preset.Icon = Icon;
        Preset.IsFavorite = IsFavorite;
        Preset.Hotkey = Hotkey;
        Preset.Monitors = [.. Monitors];

        var config = _configService.Load();
        var idx = config.Presets.FindIndex(p => p.Id == Preset.Id);
        if (idx >= 0)
            config.Presets[idx] = Preset;
        _configService.Save(config);
    }

    [RelayCommand]
    private void SaveAndApply()
    {
        Save();
        var missing = _presetService.ApplyPreset(Preset);
        IsActive = true;
    }

    [RelayCommand]
    private void Delete()
    {
        var config = _configService.Load();
        config.Presets.RemoveAll(p => p.Id == Preset.Id);
        if (config.ActivePresetId == Preset.Id)
            config.ActivePresetId = null;
        _configService.Save(config);
        BackRequested?.Invoke();
    }

    [RelayCommand]
    private void Duplicate()
    {
        var config = _configService.Load();
        var clone = new Preset
        {
            Name = Preset.Name + " (copy)",
            Icon = Preset.Icon,
            IsFavorite = false,
            Monitors = Preset.Monitors.Select(m => new MonitorConfig
            {
                DevicePath = m.DevicePath,
                EdidManufacturer = m.EdidManufacturer,
                EdidModel = m.EdidModel,
                EdidSerial = m.EdidSerial,
                Width = m.Width,
                Height = m.Height,
                RefreshRate = m.RefreshRate,
                HdrEnabled = m.HdrEnabled,
                IsCustomResolution = m.IsCustomResolution,
            }).ToList(),
        };
        config.Presets.Add(clone);
        _configService.Save(config);
    }

    [RelayCommand]
    private void SelectIcon(string iconName)
    {
        Icon = iconName;
        IsIconPickerOpen = false;
    }

    [RelayCommand]
    private void ToggleIconPicker() => IsIconPickerOpen = !IsIconPickerOpen;

    [RelayCommand]
    private void DetectMonitors()
    {
        var layout = _displayService.CaptureCurrentLayout();
        Monitors = new ObservableCollection<MonitorConfig>(layout);
        if (Monitors.Count > 0)
        {
            SelectedMonitorIndex = 0;
            SelectedMonitor = Monitors[0];
        }
    }
}
