using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using WallProjections.Models.Configuration.Interfaces;

namespace WallProjections.Models.Configuration;

public class ContentImporter : IContentImporter
{
    private const string MediaLocation = "Media";
    private const string ConfigLocation = "config.json";

    /// <summary>
    /// Backing field for <see cref="TempPath"/>
    /// </summary>
    private string? _tempPath;

    public string TempPath => _tempPath ??= Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

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

    public IConfig LoadConfig(string zipPath)
    {
        var configPath = Path.Combine(TempPath, ConfigLocation);

        try
        {
            var zipFile = ZipFile.OpenRead(zipPath);
            var configEntry = zipFile.GetEntry(ConfigLocation);

            if (configEntry is null)
            {
                throw new FileNotFoundException("config.json not in root of zip file.");
            }

            var config = JsonSerializer.Deserialize<Config>(configEntry.Open());
            if (config is null) throw new JsonException();
            return config;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public string GetHotspotFolder(Hotspot hotspot)
    {
        return Path.Combine(TempPath, MediaLocation,  hotspot.Id.ToString());
    }
}
