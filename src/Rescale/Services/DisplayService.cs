using System.Runtime.InteropServices;
using Rescale.Interop;
using Rescale.Models;
using static Rescale.Interop.DisplayConfigConstants;

namespace Rescale.Services;

/// <summary>Queries Windows display configuration: active monitors, supported modes, and HDR state.</summary>
public sealed class DisplayService
{
    /// <summary>Enumerates all currently active monitors with their device path and friendly name.</summary>
    public List<MonitorInfo> GetActiveMonitors()
    {
        var result = new List<MonitorInfo>();

        if (NativeMethods.GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out var pathCount, out var modeCount) != 0)
            return result;

        var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
        var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

        if (NativeMethods.QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, 0) != 0)
            return result;

        for (int i = 0; i < pathCount; i++)
        {
            var targetName = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
            targetName.Header.Type = DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
            targetName.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>();
            targetName.Header.AdapterId = paths[i].TargetInfo.AdapterId;
            targetName.Header.Id = paths[i].TargetInfo.Id;

            string friendlyName = "";
            string devicePath = "";
            if (NativeMethods.DisplayConfigGetDeviceInfo(ref targetName) == 0)
            {
                unsafe
                {
                    friendlyName = new string(targetName.MonitorFriendlyDeviceName).TrimEnd('\0');
                    devicePath = new string(targetName.MonitorDevicePath).TrimEnd('\0');
                }
            }

            var sourceName = new DISPLAYCONFIG_SOURCE_DEVICE_NAME();
            sourceName.Header.Type = DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
            sourceName.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SOURCE_DEVICE_NAME>();
            sourceName.Header.AdapterId = paths[i].SourceInfo.AdapterId;
            sourceName.Header.Id = paths[i].SourceInfo.Id;

            string gdiName = "";
            if (NativeMethods.DisplayConfigGetDeviceInfo(ref sourceName) == 0)
            {
                unsafe
                {
                    gdiName = new string(sourceName.ViewGdiDeviceName).TrimEnd('\0');
                }
            }

            var dm = CreateDevMode();
            NativeMethods.EnumDisplaySettings(gdiName, ENUM_CURRENT_SETTINGS, ref dm);

            var ccdRefreshRate = paths[i].TargetInfo.RefreshRate;
            int refreshHz = ccdRefreshRate.Denominator > 0
                ? (int)Math.Round((double)ccdRefreshRate.Numerator / ccdRefreshRate.Denominator)
                : (int)dm.DisplayFrequency;

            result.Add(new MonitorInfo
            {
                FriendlyName = string.IsNullOrEmpty(friendlyName) ? gdiName : friendlyName,
                DevicePath = devicePath,
                GdiDeviceName = gdiName,
                AdapterId = paths[i].TargetInfo.AdapterId,
                TargetId = paths[i].TargetInfo.Id,
                SourceId = paths[i].SourceInfo.Id,
                CurrentWidth = (int)dm.PelsWidth,
                CurrentHeight = (int)dm.PelsHeight,
                CurrentRefreshRate = refreshHz,
                PositionX = dm.PositionX,
                PositionY = dm.PositionY,
            });
        }

        return result;
    }

    /// <summary>Enumerates all supported resolutions and refresh rates for a GDI device.</summary>
    public List<DisplayMode> GetSupportedModes(string gdiDeviceName)
    {
        var modes = new List<DisplayMode>();
        var dm = CreateDevMode();

        LogService.Info($"Enumerating modes for {gdiDeviceName}");

        for (int i = 0; NativeMethods.EnumDisplaySettingsEx(gdiDeviceName, i, ref dm, EDS_RAWMODE); i++)
        {
            modes.Add(new DisplayMode
            {
                Width = (int)dm.PelsWidth,
                Height = (int)dm.PelsHeight,
                RefreshRate = (int)dm.DisplayFrequency,
            });
        }

        var unique = modes
            .DistinctBy(m => (m.Width, m.Height, m.RefreshRate))
            .OrderByDescending(m => m.Width * m.Height)
            .ThenByDescending(m => m.RefreshRate)
            .ToList();

        var nativeRes = unique.Where(m => m.Width == 3840 && m.Height == 1600).ToList();
        LogService.Info($"Total raw modes: {modes.Count}, unique: {unique.Count}");
        LogService.Info($"3840x1600 refresh rates: {string.Join(", ", nativeRes.Select(m => m.RefreshRate))}");

        var allRefreshRates = unique.Select(m => m.RefreshRate).Distinct().OrderByDescending(r => r).ToList();
        LogService.Info($"All refresh rates across all res: {string.Join(", ", allRefreshRates)}");

        return unique;
    }

    /// <summary>Enumerates modes via CCD API paths for high refresh rate support.</summary>
    public List<DisplayMode> GetSupportedModesCcd(LUID adapterId, uint targetId, string gdiDeviceName)
    {
        var modes = GetSupportedModes(gdiDeviceName);

        if (NativeMethods.GetDisplayConfigBufferSizes(QDC_ALL_PATHS, out var pathCount, out var modeCount) != 0)
            return modes;

        var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
        var ccdModes = new DISPLAYCONFIG_MODE_INFO[modeCount];

        if (NativeMethods.QueryDisplayConfig(QDC_ALL_PATHS, ref pathCount, paths, ref modeCount, ccdModes, 0) != 0)
            return modes;

        var ccdRefreshRates = new HashSet<int>();
        for (int i = 0; i < pathCount; i++)
        {
            if (paths[i].TargetInfo.AdapterId.LowPart == adapterId.LowPart &&
                paths[i].TargetInfo.AdapterId.HighPart == adapterId.HighPart &&
                paths[i].TargetInfo.Id == targetId)
            {
                var r = paths[i].TargetInfo.RefreshRate;
                if (r.Denominator > 0)
                {
                    int hz = (int)Math.Round((double)r.Numerator / r.Denominator);
                    if (hz > 0)
                        ccdRefreshRates.Add(hz);
                }
            }
        }

        LogService.Info($"CCD refresh rates for target {targetId}: {string.Join(", ", ccdRefreshRates.OrderByDescending(r => r))}");

        var nativeRes = modes.Where(m => m.Width == 3840 && m.Height == 1600).ToList();
        var existingRates = nativeRes.Select(m => m.RefreshRate).ToHashSet();

        foreach (var hz in ccdRefreshRates)
        {
            if (!existingRates.Contains(hz) && nativeRes.Count > 0)
            {
                modes.Add(new DisplayMode
                {
                    Width = nativeRes[0].Width,
                    Height = nativeRes[0].Height,
                    RefreshRate = hz,
                });
                LogService.Info($"Added CCD-only mode: {nativeRes[0].Width}x{nativeRes[0].Height}@{hz}Hz");
            }
        }

        return modes
            .DistinctBy(m => (m.Width, m.Height, m.RefreshRate))
            .OrderByDescending(m => m.Width * m.Height)
            .ThenByDescending(m => m.RefreshRate)
            .ToList();
    }

    /// <summary>Creates a snapshot of all active monitors for preset creation.</summary>
    public List<MonitorConfig> CaptureCurrentLayout()
    {
        var monitors = GetActiveMonitors();
        var configs = new List<MonitorConfig>();

        foreach (var mon in monitors)
        {
            var hdrInfo = GetHdrState(mon.AdapterId, mon.TargetId);
            configs.Add(new MonitorConfig
            {
                DevicePath = mon.DevicePath,
                Width = mon.CurrentWidth,
                Height = mon.CurrentHeight,
                RefreshRate = mon.CurrentRefreshRate,
                HdrEnabled = hdrInfo.enabled,
            });
        }

        return configs;
    }

    /// <summary>Queries the HDR (Advanced Color) support and enabled state for a display target.</summary>
    internal static (bool supported, bool enabled) GetHdrState(LUID adapterId, uint targetId)
    {
        var info = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();
        info.Header.Type = DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
        info.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();
        info.Header.AdapterId = adapterId;
        info.Header.Id = targetId;

        if (NativeMethods.DisplayConfigGetDeviceInfo(ref info) != 0)
            return (false, false);

        bool supported = (info.Value & 0x1) != 0;
        bool enabled = (info.Value & 0x2) != 0;
        return (supported, enabled);
    }

    private static DEVMODE CreateDevMode()
    {
        var dm = new DEVMODE();
        dm.Size = (ushort)Marshal.SizeOf<DEVMODE>();
        return dm;
    }
}

/// <summary>Runtime information about a currently connected display.</summary>
public class MonitorInfo
{
    public string FriendlyName { get; set; } = "";
    public string DevicePath { get; set; } = "";
    public string GdiDeviceName { get; set; } = "";
    public LUID AdapterId { get; set; }
    public uint TargetId { get; set; }
    public uint SourceId { get; set; }
    public int CurrentWidth { get; set; }
    public int CurrentHeight { get; set; }
    public int CurrentRefreshRate { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
}

/// <summary>A supported display mode (resolution + refresh rate).</summary>
public class DisplayMode
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int RefreshRate { get; set; }

    /// <summary>Reduced aspect ratio string (e.g. "16:9").</summary>
    public string AspectRatio => GcdReduce(Width, Height);

    private static string GcdReduce(int w, int h)
    {
        int a = w, b = h;
        while (b != 0) { (a, b) = (b, a % b); }
        return $"{w / a}:{h / a}";
    }
}
