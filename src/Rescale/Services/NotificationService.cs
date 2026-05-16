namespace Rescale.Services;

/// <summary>Displays toast notifications for preset application results.</summary>
public sealed class NotificationService
{
    /// <summary>Shows a notification when a preset was only partially applied due to missing monitors.</summary>
    /// <param name="presetName">Name of the applied preset.</param>
    /// <param name="missingMonitors">List of monitor identifiers that were not found.</param>
    public void ShowPartialApply(string presetName, List<string> missingMonitors)
    {
        var monitorList = string.Join(", ", missingMonitors);
        var message = $"Monitor {monitorList} not found. Configuration applied to available displays.";

        // TODO: Integrate with Windows Toast API or WPF UI Snackbar
        System.Diagnostics.Debug.WriteLine($"[Toast] Preset applied partially: {presetName} — {message}");
    }

    /// <summary>Shows a confirmation notification after successful preset application.</summary>
    /// <param name="presetName">Name of the applied preset.</param>
    public void ShowPresetApplied(string presetName)
    {
        System.Diagnostics.Debug.WriteLine($"[Toast] Preset applied: {presetName}");
    }
}
