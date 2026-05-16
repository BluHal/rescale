using System.Windows;
using Rescale.Services;
using Rescale.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace Rescale;

/// <summary>Application entry point. Composes services, view models, and the main window.</summary>
public partial class App : Application
{
    private readonly ConfigService _configService = new();
    private readonly DisplayService _displayService = new();
    private readonly ResolutionService _resolutionService = new();
    private readonly HdrService _hdrService = new();
    private readonly MonitorIdentifier _monitorIdentifier = new();
    private TrayService? _trayService;

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

        var presetService = new PresetService(
            _displayService, _resolutionService, _hdrService, _monitorIdentifier, _configService);

        var mainVm = new MainViewModel(
            _configService, presetService, _displayService, _resolutionService, _hdrService);

        var config = _configService.Load();
        ApplyTheme(config.Theme);

        var mainWindow = new MainWindow(mainVm);
        MainWindow = mainWindow;

        _trayService = new TrayService(presetService, _configService, mainWindow);
        _trayService.Initialize();

        mainWindow.Show();
    }

    /// <summary>Applies the specified theme to the WPF UI appearance manager.</summary>
    public void ApplyTheme(string theme)
    {
        var appTheme = theme switch
        {
            "Dark" => ApplicationTheme.Dark,
            "Light" => ApplicationTheme.Light,
            _ => ApplicationTheme.Dark,
        };
        ApplicationThemeManager.Apply(appTheme);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayService?.Dispose();
        base.OnExit(e);
    }
}
