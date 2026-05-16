namespace Rescale.Models;

/// <summary>Display settings for a single monitor within a preset.</summary>
public sealed class MonitorConfig
{
    /// <summary>Windows display device path used as primary identifier.</summary>
    public string DevicePath { get; set; } = "";

    /// <summary>EDID manufacturer PNP ID (3-letter code, e.g. "DEL").</summary>
    public string EdidManufacturer { get; set; } = "";

    /// <summary>EDID monitor model name (fallback identifier).</summary>
    public string EdidModel { get; set; } = "";

    /// <summary>EDID serial number string.</summary>
    public string EdidSerial { get; set; } = "";

    /// <summary>Horizontal resolution in pixels.</summary>
    public int Width { get; set; }

    /// <summary>Vertical resolution in pixels.</summary>
    public int Height { get; set; }

    /// <summary>Display refresh rate in Hz.</summary>
    public int RefreshRate { get; set; }

    /// <summary>Whether HDR (Advanced Color) should be enabled.</summary>
    public bool HdrEnabled { get; set; }

    /// <summary>Whether this resolution is not reported by the display driver.</summary>
    public bool IsCustomResolution { get; set; }
}
