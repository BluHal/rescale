using System.Runtime.InteropServices;
using Rescale.Interop;
using static Rescale.Interop.DisplayConfigConstants;

namespace Rescale.Services;

/// <summary>Changes display resolution and refresh rate via Win32 APIs.</summary>
public sealed class ResolutionService
{
    /// <summary>
    /// Changes resolution and refresh rate for a specific display.
    /// Uses ChangeDisplaySettingsEx first, falls back to SetDisplayConfig for cross-aspect-ratio switches.
    /// </summary>
    /// <param name="gdiDeviceName">GDI device name (e.g. \\.\DISPLAY1).</param>
    /// <param name="width">Target horizontal resolution in pixels.</param>
    /// <param name="height">Target vertical resolution in pixels.</param>
    /// <param name="refreshRate">Target refresh rate in Hz.</param>
    /// <returns>True if the resolution was changed successfully.</returns>
    public bool SetResolution(string gdiDeviceName, int width, int height, int refreshRate)
    {
        var dm = new DEVMODE();
        dm.Size = (ushort)Marshal.SizeOf<DEVMODE>();
        dm.PelsWidth = (uint)width;
        dm.PelsHeight = (uint)height;
        dm.DisplayFrequency = (uint)refreshRate;
        dm.Fields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY;

        int result = NativeMethods.ChangeDisplaySettingsEx(gdiDeviceName, ref dm, 0, CDS_UPDATEREGISTRY, 0);
        if (result == DISP_CHANGE_SUCCESSFUL)
            return true;

        return SetResolutionViaSdc(width, height, refreshRate);
    }

    /// <summary>
    /// Fallback resolution change via SetDisplayConfig. Modifies source mode dimensions
    /// in the active display topology and re-applies refresh rate after a delay.
    /// </summary>
    private static bool SetResolutionViaSdc(int width, int height, int refreshRate)
    {
        if (NativeMethods.GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out var pathCount, out var modeCount) != 0)
            return false;

        var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
        var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

        if (NativeMethods.QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, 0) != 0)
            return false;

        bool modified = false;
        for (int i = 0; i < modeCount; i++)
        {
            if (modes[i].InfoType == MODE_INFO_TYPE_SOURCE)
            {
                unsafe
                {
                    fixed (byte* data = modes[i].Data)
                    {
                        *(uint*)data = (uint)width;
                        *(uint*)(data + 4) = (uint)height;
                    }
                }
                modified = true;
            }
        }

        if (!modified)
            return false;

        uint flags = SDC_APPLY | SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_SAVE_TO_DATABASE | SDC_ALLOW_CHANGES;
        int r = NativeMethods.SetDisplayConfig(pathCount, paths, modeCount, modes, flags);
        if (r != 0)
            return false;

        // SDC resets to 60Hz — re-apply desired refresh rate
        Thread.Sleep(200);
        var dm = new DEVMODE();
        dm.Size = (ushort)Marshal.SizeOf<DEVMODE>();
        dm.PelsWidth = (uint)width;
        dm.PelsHeight = (uint)height;
        dm.DisplayFrequency = (uint)refreshRate;
        dm.Fields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY;

        NativeMethods.ChangeDisplaySettingsEx(null, ref dm, 0, CDS_UPDATEREGISTRY, 0);
        return true;
    }
}
