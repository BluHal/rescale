using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rescale.Models;

namespace Rescale.Services;

/// <summary>Loads and saves <see cref="AppConfig"/> as JSON in %APPDATA%\Rescale.</summary>
public sealed class ConfigService
{
    private static readonly string ConfigDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rescale");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>Loads the configuration from disk, returning defaults if the file does not exist.</summary>
    public AppConfig Load()
    {
        if (!File.Exists(ConfigPath))
            return new AppConfig();

        var json = File.ReadAllText(ConfigPath);
        return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
    }

    /// <summary>Persists the configuration to disk, creating the directory if needed.</summary>
    public void Save(AppConfig config)
    {
        Directory.CreateDirectory(ConfigDir);
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigPath, json);
    }
}
