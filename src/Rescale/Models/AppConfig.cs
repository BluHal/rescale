namespace Rescale.Models;

/// <summary>Root configuration model persisted to %APPDATA%\Rescale\config.json.</summary>
public sealed class AppConfig
{
    /// <summary>All user-defined display presets.</summary>
    public List<Preset> Presets { get; set; } = [];

    /// <summary>Application theme: "System", "Dark", or "Light".</summary>
    public string Theme { get; set; } = "System";

    /// <summary>Whether the application should launch at Windows startup.</summary>
    public bool AutoStart { get; set; }

    /// <summary>Global hotkey string for cycling presets (e.g. "Ctrl+Alt+Space").</summary>
    public string? CycleHotkey { get; set; }

    /// <summary>ID of the currently active preset, or null if none.</summary>
    public string? ActivePresetId { get; set; }
}
