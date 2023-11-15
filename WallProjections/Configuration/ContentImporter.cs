using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace WallProjections.Configuration;

public class ContentImporter
{
    public static Config Load(string zipPath)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        while (Directory.Exists(tempPath))
        {
            tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        var folderInfo = Directory.CreateDirectory(tempPath);

        ZipFile.ExtractToDirectory(zipPath, tempPath);

        var config = LoadConfig(tempPath, "config.json");
        return config;
    }

    /// <summary>
    /// Cleans up the temporary folder.
    /// </summary>
    /// <param name="config">The Config class used to access the files.</param>
    public static void Cleanup(Config config)
    {
        Directory.Delete(config.TempPath);
    }

    /// <summary>
    /// Loads a config from a .json file.
    /// </summary>
    /// <param name="configLocation">Name of configuration file.</param>
    /// <param name="tempPath">Path to temporary folder to use.</param>
    /// <returns>Loaded Config.</returns>
    /// <exception cref="JsonException">Format of config file is invalid.</exception>
    public static Config LoadConfig(string tempPath, string configLocation)
    {
        var configPath = Path.Combine(tempPath, configLocation);

        // Create default config if none exists.
        if (!File.Exists(configPath))
        {
            var newConfig = new Config(new List<Hotspot>());
            newConfig.TempPath = tempPath;
            newConfig.SaveConfig();
            return newConfig;
        }

        try
        {
            var configJson = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<Config>(configJson);
            if (config is null) throw new JsonException();
            config.TempPath = tempPath;
            return config;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
