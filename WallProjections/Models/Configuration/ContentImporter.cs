using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using WallProjections.Models.Configuration.Interfaces;

namespace WallProjections.Models.Configuration;

public class ContentImporter : IContentImporter
{
    private const string MediaLocation = "Media";
    private const string ConfigFileName = "config.json";

    /// <summary>
    /// Backing field for <see cref="TempPath"/>
    /// </summary>
    private string? _tempPath;

    private string TempPath => _tempPath ??= Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public IConfig Load(string zipPath)
    {
        // Clean up existing directly if in use.
        if (Directory.Exists(TempPath))
        {
            Directory.Delete(TempPath, true);
        }

        Directory.CreateDirectory(TempPath);

        ZipFile.ExtractToDirectory(zipPath, TempPath);

        var config = LoadConfig(zipPath);
        return config;
    }

    public void Cleanup()
    {
        Directory.Delete(TempPath, true);
    }

    /// <summary>
    /// Loads a config from a .json file.
    /// </summary>
    /// <param name="zipPath">Path to zip containing config.json.</param>
    /// <returns>Loaded Config.</returns>
    /// <exception cref="JsonException">Format of config file is invalid.</exception>
    /// <exception cref="FileNotFoundException">If config file cannot be found in zip file.</exception>
    /// TODO: More effective error handling of invalid/missing config files.
    private static IConfig LoadConfig(string zipPath)
    {
        var zipFile = ZipFile.OpenRead(zipPath);
        var configEntry = zipFile.GetEntry(ConfigFileName);

        if (configEntry is null)
        {
            throw new FileNotFoundException("config.json not in root of zip file.");
        }

        var config = JsonSerializer.Deserialize<Config>(configEntry.Open());
        if (config is null) throw new JsonException();
        return config;
    }

    public string GetHotspotMediaFolder(Hotspot hotspot)
    {
        return Path.Combine(TempPath, MediaLocation,  hotspot.Id.ToString());
    }
}
