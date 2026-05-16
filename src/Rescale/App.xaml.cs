using System.Windows;
using Rescale.Services;
using Rescale.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Rescale;

/// <summary>Application entry point. Composes services, view models, and the main window.</summary>
public partial class App : Application
{
    private readonly ConfigService _configService = new();
    private readonly DisplayService _displayService = new();
    private readonly ResolutionService _resolutionService = new();
    private readonly HdrService _hdrService = new();
    private readonly MonitorIdentifier _monitorIdentifier = new();
    private readonly HotkeyService _hotkeyService = new();
    private TrayService? _trayService;
    private PresetService? _presetService;
    private int? _cycleHotkeyId;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            LogService.Error("Unhandled UI exception", args.Exception);
            args.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            LogService.Error("Unhandled domain exception", args.ExceptionObject as Exception);
        };

        LogService.Info("App starting");

        _presetService = new PresetService(
            _displayService, _resolutionService, _hdrService, _monitorIdentifier, _configService);

        var mainVm = new MainViewModel(
            _configService, _presetService, _displayService, _resolutionService, _hdrService);

        var config = _configService.Load();
        ApplyTheme(config.Theme);

        var mainWindow = new MainWindow(mainVm);
        MainWindow = mainWindow;

        _trayService = new TrayService(_presetService, _configService, mainWindow);
        _trayService.Initialize();

        mainWindow.Loaded += (_, _) =>
        {
            _hotkeyService.Initialize(mainWindow);
            RegisterCycleHotkey(config.CycleHotkey);
        };

        mainWindow.Show();
    }

    /// <summary>Applies the specified theme to the WPF UI appearance manager.</summary>
    public void ApplyTheme(string theme)
    {
        var appTheme = theme switch
        {
            "Dark" => ApplicationTheme.Dark,
            "Light" => ApplicationTheme.Light,
            _ => SystemThemeManager.GetCachedSystemTheme() == SystemTheme.Dark
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light,
        };
        ApplicationThemeManager.Apply(appTheme, WindowBackdropType.Mica, true);
    }

    /// <summary>Registers or re-registers the global cycle hotkey.</summary>
    public void UpdateCycleHotkey(string? hotkey)
    {
        RegisterCycleHotkey(hotkey);
    }

    private void RegisterCycleHotkey(string? hotkey)
    {
        if (_cycleHotkeyId.HasValue)
        {
            _hotkeyService.Unregister(_cycleHotkeyId.Value);
            _cycleHotkeyId = null;
        }

        if (string.IsNullOrEmpty(hotkey) || _presetService == null || _trayService == null)
            return;

        _cycleHotkeyId = _hotkeyService.Register(hotkey, () =>
        {
            Dispatcher.Invoke(() =>
            {
                var (applied, _) = _presetService.CycleNext();
                if (applied != null)
                    _trayService.UpdateTray(applied);
            });
        });

        LogService.Info(_cycleHotkeyId.HasValue
            ? $"Cycle hotkey registered: {hotkey}"
            : $"Failed to register cycle hotkey: {hotkey}");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyService.Dispose();
        _trayService?.Dispose();
        base.OnExit(e);
    }
}
