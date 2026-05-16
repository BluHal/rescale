namespace Rescale.Models;

/// <summary>A named snapshot of display settings for all monitors.</summary>
public sealed class Preset
{
    /// <summary>Unique identifier (hex GUID without dashes).</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>User-visible preset name.</summary>
    public string Name { get; set; } = "Nuovo preset";

    /// <summary>Icon glyph name from the icon picker (e.g. "Game", "Monitor").</summary>
    public string Icon { get; set; } = "";

    /// <summary>Whether this preset appears in the tray cycle rotation.</summary>
    public bool IsFavorite { get; set; } = true;

    /// <summary>Sort order within the favorites list.</summary>
    public int Order { get; set; }

    /// <summary>Direct hotkey string for applying this preset (e.g. "Ctrl+Alt+1").</summary>
    public string? Hotkey { get; set; }

    /// <summary>Per-monitor display configurations in this preset.</summary>
    public List<MonitorConfig> Monitors { get; set; } = [];
}
