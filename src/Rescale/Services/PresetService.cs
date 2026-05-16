using Rescale.Models;

namespace Rescale.Services;

/// <summary>Orchestrates preset application, cycling, and capture across all display services.</summary>
public sealed class PresetService(
    DisplayService displayService,
    ResolutionService resolutionService,
    HdrService hdrService,
    MonitorIdentifier monitorIdentifier,
    ConfigService configService)
{
    /// <summary>Applies a preset to all matching monitors. Returns names of missing monitors.</summary>
    /// <param name="preset">The preset to apply.</param>
    /// <returns>List of monitor identifiers that could not be matched to a connected display.</returns>
    public List<string> ApplyPreset(Preset preset)
    {
        var activeMonitors = displayService.GetActiveMonitors();
        var missing = new List<string>();

        var hdrActions = new List<Action>();

        foreach (var monConfig in preset.Monitors)
        {
            var match = monitorIdentifier.FindMatch(monConfig, activeMonitors);
            if (match == null)
            {
                missing.Add(string.IsNullOrEmpty(monConfig.EdidModel) ? monConfig.DevicePath : monConfig.EdidModel);
                continue;
            }

            resolutionService.SetResolution(match.AdapterId, match.TargetId, match.GdiDeviceName, monConfig.Width, monConfig.Height, monConfig.RefreshRate);

            var m = match;
            var hdrEnabled = monConfig.HdrEnabled;
            hdrActions.Add(() => hdrService.SetHdr(m.AdapterId, m.TargetId, hdrEnabled));
        }

        if (hdrActions.Count > 0)
        {
            Thread.Sleep(500);
            foreach (var action in hdrActions)
                action();
        }

        var config = configService.Load();
        config.ActivePresetId = preset.Id;
        configService.Save(config);

        return missing;
    }

    /// <summary>Cycles to the next favorite preset and applies it.</summary>
    /// <returns>The applied preset and any missing monitor names, or null if no favorites exist.</returns>
    public (Preset? applied, List<string> missing) CycleNext()
    {
        var config = configService.Load();
        var favorites = config.Presets
            .Where(p => p.IsFavorite)
            .OrderBy(p => p.Order)
            .ToList();

        if (favorites.Count == 0)
            return (null, []);

        int currentIdx = favorites.FindIndex(p => p.Id == config.ActivePresetId);
        int nextIdx = (currentIdx + 1) % favorites.Count;
        var next = favorites[nextIdx];

        var missing = ApplyPreset(next);
        return (next, missing);
    }

    /// <summary>Creates a new preset as a snapshot of the current display configuration.</summary>
    /// <param name="name">Name for the new preset.</param>
    /// <returns>The newly created and persisted preset.</returns>
    public Preset CaptureCurrentAsPreset(string name)
    {
        var config = configService.Load();
        var monitors = displayService.CaptureCurrentLayout();

        var preset = new Preset
        {
            Name = name,
            Icon = "",
            IsFavorite = true,
            Order = config.Presets.Count(p => p.IsFavorite),
            Monitors = monitors,
        };

        config.Presets.Add(preset);
        configService.Save(config);
        return preset;
    }
}
