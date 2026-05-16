using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Rescale.Models;
using Rescale.Services;

namespace Rescale.ViewModels;

/// <summary>View model for the settings page: theme, autostart, and cycle hotkey.</summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly AppConfig _config;
    private readonly ConfigService _configService;

    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    [ObservableProperty]
    private string _theme;

    [ObservableProperty]
    private bool _autoStart;

    [ObservableProperty]
    private string? _cycleHotkey;

    public SettingsViewModel(AppConfig config, ConfigService configService)
    {
        _config = config;
        _configService = configService;
        _theme = config.Theme;
        _autoStart = config.AutoStart;
        _cycleHotkey = config.CycleHotkey;
    }

    partial void OnThemeChanged(string value)
    {
        _config.Theme = value;
        _configService.Save(_config);
    }

    partial void OnAutoStartChanged(bool value)
    {
        _config.AutoStart = value;
        _configService.Save(_config);
        SetRegistryAutoStart(value);
    }

    partial void OnCycleHotkeyChanged(string? value)
    {
        _config.CycleHotkey = value;
        _configService.Save(_config);
    }

    private static void SetRegistryAutoStart(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
        if (key == null) return;

        if (enabled)
        {
            var exe = Environment.ProcessPath ?? "";
            key.SetValue("Rescale", $"\"{exe}\"");
        }
        else
        {
            key.DeleteValue("Rescale", false);
        }
    }

    [RelayCommand]
    private void SetTheme(string theme) => Theme = theme;
}
