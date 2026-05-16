# Rescale

Windows system tray tool for switching between display resolution presets. Cycle through saved configurations with a single click or a global hotkey.

## Features

- **Preset management** — Create, edit, duplicate, and delete display presets. Each preset stores resolution, refresh rate, and HDR state for every connected monitor.
- **System tray cycling** — Left-click the tray icon to cycle through favorite presets. Right-click for the full context menu.
- **Global hotkeys** — Assign a hotkey to each preset or a global cycle key in settings.
- **Multi-monitor support** — Configure each monitor independently within a preset. Monitors are identified by device path (primary) and EDID data (fallback).
- **HDR control** — Toggle HDR (Advanced Color) per monitor per preset.
- **Fluent Design UI** — Modern Windows 11 look using WPF UI (Fluent Design).
- **Minimize to tray** — Closing the window minimizes to the system tray. Exit from the tray menu.
- **Autostart** — Optional "Start with Windows" via registry key.

## Screenshots

*Coming soon*

## Requirements

- Windows 10 21H2+ or Windows 11
- .NET 10 runtime (or use the self-contained publish)

## Building

```bash
dotnet build src/Rescale/Rescale.csproj
```

## Publishing

Single-file, self-contained, trimmed executable:

```bash
dotnet publish src/Rescale/Rescale.csproj -c Release
```

Output: `src/Rescale/bin/Release/net10.0-windows/win-x64/publish/Rescale.exe`

## Tech Stack

| Component | Choice |
|-----------|--------|
| Language | C# |
| Runtime | .NET 10 |
| UI | WPF + [WPF UI](https://github.com/lepoco/wpfui) (Fluent Design) |
| MVVM | [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) |
| Tray icon | [H.NotifyIcon.Wpf](https://github.com/HavenDV/H.NotifyIcon) |
| Persistence | JSON in `%APPDATA%\Rescale\config.json` |
| Display APIs | Win32 `QueryDisplayConfig`, `SetDisplayConfig`, `ChangeDisplaySettingsEx` |
| HDR | `DisplayConfigSetDeviceInfo` with `DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE` |

## Project Structure

```
src/Rescale/
  Models/          — AppConfig, Preset, MonitorConfig
  Services/        — Display, Resolution, HDR, Config, Preset, Tray, Hotkey, Notification, MonitorIdentifier
  ViewModels/      — Main, PresetList, PresetDetail, Settings
  Views/           — PresetListPage, PresetDetailPage, SettingsPage
  Controls/        — MonitorLayoutControl, IconPickerControl, HotkeyPickerControl
  Interop/         — NativeMethods (P/Invoke), DisplayConfigStructs, EdidParser
  Assets/          — rescale.ico
```

## Configuration

Settings are stored in `%APPDATA%\Rescale\config.json` and include:

- Display presets with per-monitor resolution, refresh rate, and HDR state
- Theme preference (System / Dark / Light)
- Autostart toggle
- Global cycle hotkey
- Per-preset direct hotkeys

## License

MIT
