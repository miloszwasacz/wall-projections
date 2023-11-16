using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using WallProjections.Models.Configuration.Interfaces;

namespace WallProjections.Models.Configuration;

public class ContentImporter
{
    /// <summary>
    /// Load a zip file of the config.json file and the media files.
    /// </summary>
    /// <param name="zipPath">Path to the zip file</param>
    /// <returns>Config with the loaded</returns>
    public static IConfig Load(string zipPath)
    {
        Directory.CreateDirectory(Config.TempPath);
        ZipFile.ExtractToDirectory(zipPath, Config.TempPath);

        var config = LoadConfig(Config.TempPath, "config.json");
        return config;
    }

    /// <summary>
    /// Cleans up the temporary folder.
    /// </summary>
    /// <param name="config">The Config class used to access the files.</param>
    public static void Cleanup(IConfig config)
    {
        Directory.Delete(Config.TempPath, true);
    }

    /// <summary>
    /// Loads a config from a .json file.
    /// </summary>
    /// <param name="configLocation">Name of configuration file.</param>
    /// <param name="tempPath">Path to temporary folder to use.</param>
    /// <returns>Loaded Config.</returns>
    /// <exception cref="JsonException">Format of config file is invalid.</exception>
    public static IConfig LoadConfig(string tempPath, string configLocation)
    {
        var configPath = Path.Combine(tempPath, configLocation);

        // Create default config if none exists.
        if (!File.Exists(configPath))
        {
            var newConfig = new Config(new List<Hotspot>());
            newConfig.SaveConfig();
            return newConfig;
        }

        try
        {
            var configJson = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<Config>(configJson);
            if (config is null) throw new JsonException();
            return config;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
