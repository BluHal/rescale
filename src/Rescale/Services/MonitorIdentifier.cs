using Rescale.Models;

namespace Rescale.Services;

/// <summary>Matches saved monitor configurations to currently connected physical displays.</summary>
public sealed class MonitorIdentifier
{
    /// <summary>
    /// Matches a <see cref="MonitorConfig"/> from a preset to a currently active monitor.
    /// Primary match: DevicePath. Fallback: EDID manufacturer + model.
    /// </summary>
    /// <param name="config">The saved monitor configuration to match.</param>
    /// <param name="activeMonitors">Currently connected monitors.</param>
    /// <returns>The matching <see cref="MonitorInfo"/>, or null if no match is found.</returns>
    public MonitorInfo? FindMatch(MonitorConfig config, List<MonitorInfo> activeMonitors)
    {
        var byPath = activeMonitors.FirstOrDefault(m =>
            !string.IsNullOrEmpty(config.DevicePath) &&
            string.Equals(m.DevicePath, config.DevicePath, StringComparison.OrdinalIgnoreCase));

        if (byPath != null)
            return byPath;

        if (!string.IsNullOrEmpty(config.EdidManufacturer) &&
            !string.IsNullOrEmpty(config.EdidModel))
        {
            return activeMonitors.FirstOrDefault(m =>
                m.FriendlyName.Contains(config.EdidModel, StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }
}
