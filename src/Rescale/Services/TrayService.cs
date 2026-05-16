using System.Drawing;
using System.Windows;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Rescale.Models;

namespace Rescale.Services;

/// <summary>Manages the system tray icon, context menu, and left-click preset cycling.</summary>
public sealed class TrayService : IDisposable
{
    private readonly PresetService _presetService;
    private readonly ConfigService _configService;
    private readonly Window _mainWindow;
    private TaskbarIcon? _trayIcon;

    public TrayService(PresetService presetService, ConfigService configService, Window mainWindow)
    {
        _presetService = presetService;
        _configService = configService;
        _mainWindow = mainWindow;
    }

    /// <summary>Creates and shows the tray icon with its context menu.</summary>
    public void Initialize()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = BuildTooltip(),
            Icon = RenderTrayIcon("R"),
            ContextMenu = BuildContextMenu(),
        };

        _trayIcon.TrayLeftMouseDown += OnLeftClick;
    }

    private void OnLeftClick(object? sender, RoutedEventArgs e)
    {
        var (applied, missing) = _presetService.CycleNext();
        if (applied != null)
        {
            UpdateTray(applied);
            if (missing.Count > 0)
                ShowMissingNotification(applied.Name, missing);
        }
    }

    /// <summary>Updates the tray icon glyph, tooltip, and context menu for a newly active preset.</summary>
    public void UpdateTray(Preset activePreset)
    {
        if (_trayIcon == null) return;
        _trayIcon.ToolTipText = BuildTooltip();
        _trayIcon.Icon = RenderTrayIcon(activePreset.Name.Length > 0 ? activePreset.Name[..1] : "R");
        _trayIcon.ContextMenu = BuildContextMenu();
    }

    private string BuildTooltip()
    {
        var config = _configService.Load();
        var active = config.Presets.FirstOrDefault(p => p.Id == config.ActivePresetId);
        if (active == null)
            return "Rescale";

        var primary = active.Monitors.FirstOrDefault();
        if (primary == null)
            return active.Name;

        var hdr = primary.HdrEnabled ? "HDR" : "SDR";
        return $"{active.Name} — {primary.Width}x{primary.Height} {primary.RefreshRate}Hz {hdr}";
    }

    private System.Windows.Controls.ContextMenu BuildContextMenu()
    {
        var menu = new System.Windows.Controls.ContextMenu();
        var config = _configService.Load();
        var active = config.Presets.FirstOrDefault(p => p.Id == config.ActivePresetId);

        var header = new System.Windows.Controls.MenuItem
        {
            Header = active != null ? $"{active.Name}" : "No active preset",
            IsEnabled = false,
        };
        menu.Items.Add(header);
        menu.Items.Add(new System.Windows.Controls.Separator());

        foreach (var preset in config.Presets.Where(p => p.IsFavorite).OrderBy(p => p.Order))
        {
            var item = new System.Windows.Controls.MenuItem
            {
                Header = preset.Name,
                IsCheckable = true,
                IsChecked = preset.Id == config.ActivePresetId,
            };
            var p = preset;
            item.Click += (_, _) =>
            {
                _presetService.ApplyPreset(p);
                UpdateTray(p);
            };
            menu.Items.Add(item);
        }

        menu.Items.Add(new System.Windows.Controls.Separator());

        var openItem = new System.Windows.Controls.MenuItem { Header = "Open configuration" };
        openItem.Click += (_, _) =>
        {
            _mainWindow.Show();
            _mainWindow.Activate();
            if (_mainWindow.WindowState == WindowState.Minimized)
                _mainWindow.WindowState = WindowState.Normal;
        };
        menu.Items.Add(openItem);

        var autoItem = new System.Windows.Controls.MenuItem
        {
            Header = "Start with Windows",
            IsCheckable = true,
            IsChecked = config.AutoStart,
        };
        autoItem.Click += (_, _) =>
        {
            config.AutoStart = !config.AutoStart;
            _configService.Save(config);
        };
        menu.Items.Add(autoItem);

        menu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem { Header = "Quit Rescale" };
        exitItem.Click += (_, _) => Application.Current.Shutdown();
        menu.Items.Add(exitItem);

        return menu;
    }

    /// <summary>Renders a 64x64 tray icon with a glyph on a blue rounded rectangle.</summary>
    private static Icon RenderTrayIcon(string glyph)
    {
        using var bmp = new Bitmap(64, 64);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        using var bgBrush = new SolidBrush(Color.FromArgb(0, 103, 192));
        g.FillRoundedRectangle(bgBrush, new Rectangle(2, 2, 60, 60), 12);

        using var font = new Font("Segoe UI", 22, System.Drawing.FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        var size = g.MeasureString(glyph, font);
        g.DrawString(glyph, font, textBrush,
            (64 - size.Width) / 2,
            (64 - size.Height) / 2);

        return System.Drawing.Icon.FromHandle(bmp.GetHicon());
    }

    private static void ShowMissingNotification(string presetName, List<string> missing)
    {
        System.Diagnostics.Debug.WriteLine(
            $"Preset '{presetName}' applied partially. Missing: {string.Join(", ", missing)}");
    }

    /// <summary>Disposes the tray icon and releases its resources.</summary>
    public void Dispose()
    {
        _trayIcon?.Dispose();
    }
}

/// <summary>Extension methods for System.Drawing.Graphics.</summary>
internal static class GraphicsExtensions
{
    /// <summary>Fills a rectangle with rounded corners.</summary>
    public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        g.FillPath(brush, path);
    }
}
