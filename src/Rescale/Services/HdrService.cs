using System.Runtime.InteropServices;
using Rescale.Interop;
using static Rescale.Interop.DisplayConfigConstants;

namespace Rescale.Services;

/// <summary>Controls HDR (Advanced Color) state on display targets via DisplayConfigSetDeviceInfo.</summary>
public sealed class HdrService
{
    /// <summary>Enables or disables HDR on a specific display target.</summary>
    /// <param name="adapterId">Display adapter LUID.</param>
    /// <param name="targetId">Display target ID.</param>
    /// <param name="enabled">True to enable HDR, false to disable.</param>
    /// <returns>True if the operation succeeded.</returns>
    public bool SetHdr(LUID adapterId, uint targetId, bool enabled)
    {
        var state = new DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE();
        state.Header.Type = DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE;
        state.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE>();
        state.Header.AdapterId = adapterId;
        state.Header.Id = targetId;
        state.Value = enabled ? 1u : 0u;

        return NativeMethods.DisplayConfigSetDeviceInfo(ref state) == 0;
    }

    /// <summary>Enables or disables HDR on all active displays.</summary>
    /// <param name="enabled">True to enable HDR, false to disable.</param>
    /// <returns>True if at least one display was updated successfully.</returns>
    public bool SetHdrAll(bool enabled)
    {
        if (NativeMethods.GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out var pathCount, out var modeCount) != 0)
            return false;

        var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
        var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

        if (NativeMethods.QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, 0) != 0)
            return false;

        bool anySuccess = false;
        for (int i = 0; i < pathCount; i++)
        {
            if (SetHdr(paths[i].TargetInfo.AdapterId, paths[i].TargetInfo.Id, enabled))
                anySuccess = true;
        }

        return anySuccess;
    }
}
