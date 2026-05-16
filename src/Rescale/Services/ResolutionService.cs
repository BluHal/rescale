using System.Runtime.InteropServices;
using Rescale.Interop;
using static Rescale.Interop.DisplayConfigConstants;

namespace Rescale.Services;

/// <summary>Changes display resolution and refresh rate via CCD (SetDisplayConfig) API.</summary>
public sealed class ResolutionService
{
    /// <summary>
    /// Changes resolution and refresh rate for a specific display using the CCD API.
    /// Falls back to ChangeDisplaySettingsEx if CCD fails.
    /// </summary>
    public bool SetResolution(LUID adapterId, uint targetId, string gdiDeviceName, int width, int height, int refreshRate)
    {
        LogService.Info($"SetResolution: {gdiDeviceName} -> {width}x{height}@{refreshRate}Hz");

        if (SetResolutionViaCcd(adapterId, targetId, width, height, refreshRate))
        {
            LogService.Info("CCD apply succeeded");
            return true;
        }

        LogService.Info("CCD failed, trying legacy");
        return SetResolutionLegacy(gdiDeviceName, width, height, refreshRate);
    }

    private static bool SetResolutionViaCcd(LUID adapterId, uint targetId, int width, int height, int refreshRate)
    {
        if (NativeMethods.GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out var pathCount, out var modeCount) != 0)
            return false;

        var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
        var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

        if (NativeMethods.QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, 0) != 0)
            return false;

        int pathIdx = -1;
        for (int i = 0; i < pathCount; i++)
        {
            if (paths[i].TargetInfo.AdapterId.LowPart == adapterId.LowPart &&
                paths[i].TargetInfo.AdapterId.HighPart == adapterId.HighPart &&
                paths[i].TargetInfo.Id == targetId)
            {
                pathIdx = i;
                break;
            }
        }

        if (pathIdx < 0)
        {
            LogService.Error($"CCD: path not found for target {targetId}");
            return false;
        }

        var currentRate = paths[pathIdx].TargetInfo.RefreshRate;
        LogService.Info($"CCD: current path rate = {currentRate.Numerator}/{currentRate.Denominator}");

        paths[pathIdx].TargetInfo.RefreshRate = new DISPLAYCONFIG_RATIONAL
        {
            Numerator = (uint)refreshRate,
            Denominator = 1,
        };

        uint sourceModeIdx = paths[pathIdx].SourceInfo.ModeInfoIdx;
        if (sourceModeIdx < modeCount && modes[sourceModeIdx].InfoType == MODE_INFO_TYPE_SOURCE)
        {
            unsafe
            {
                fixed (byte* data = modes[sourceModeIdx].Data)
                {
                    *(uint*)data = (uint)width;
                    *(uint*)(data + 4) = (uint)height;
                }
            }
        }

        // Invalidate target mode index so the system recalculates timing for the new refresh rate
        paths[pathIdx].TargetInfo.ModeInfoIdx = 0xFFFFFFFF;

        uint flags = SDC_APPLY | SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_SAVE_TO_DATABASE | SDC_ALLOW_CHANGES;
        int result = NativeMethods.SetDisplayConfig(pathCount, paths, modeCount, modes, flags);
        LogService.Info($"CCD: SetDisplayConfig returned {result}");
        return result == 0;
    }

    private static bool SetResolutionLegacy(string gdiDeviceName, int width, int height, int refreshRate)
    {
        var dm = new DEVMODE();
        dm.Size = (ushort)Marshal.SizeOf<DEVMODE>();
        dm.PelsWidth = (uint)width;
        dm.PelsHeight = (uint)height;
        dm.DisplayFrequency = (uint)refreshRate;
        dm.Fields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY;

        int result = NativeMethods.ChangeDisplaySettingsEx(gdiDeviceName, ref dm, 0, CDS_UPDATEREGISTRY, 0);
        LogService.Info($"Legacy: ChangeDisplaySettingsEx returned {result}");
        return result == DISP_CHANGE_SUCCESSFUL;
    }
}
