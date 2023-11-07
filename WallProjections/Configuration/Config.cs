using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WallProjections.Configuration;

/// <summary>
/// Stores all user customisable configuration for the program.
/// </summary>
[Serializable]
public class Config
{
    /// <summary>
    /// List of all hotspots (their locations and content).
    /// </summary>
    [JsonInclude]
    public List<Hotspot> Hotspots { get; set; }

    /// <summary>
    /// Location where to store configuration.
    /// </summary>
    public string ConfigLocation { get; set; }

    /// <summary>
    /// Default constructor for Config.
    /// </summary>
    public Config()
    {
        Hotspots = new List<Hotspot>();
        ConfigLocation = "config.json";
    }

    /// <summary>
    /// Creates Config object with no hotspots but has a custom save location.
    /// </summary>
    /// <param name="configLocation">Location to save configuration.</param>
    public Config(string configLocation)
    {
        ConfigLocation = configLocation;
        Hotspots = new List<Hotspot>();
    }

    /// <summary>
    /// Constructs a new Config object using list of hotspots.
    /// </summary>
    /// <param name="hotspots">List of hotspots to create config with.</param>
    public Config(List<Hotspot> hotspots)
    {
        ConfigLocation = "config.json";
        Hotspots = hotspots;
    }

    /// <summary>
    /// Constructs a new Config object using list of hotspots and a custom location.
    /// </summary>
    /// <param name="hotspots">List of hotspots to create config with.</param>
    /// <param name="configLocation">Path to config location.</param>
    public Config(List<Hotspot> hotspots, string configLocation)
    {
        ConfigLocation = configLocation;
        Hotspots = hotspots;
    }

    /// <summary>
    /// Saves the current state of Config to the file path from ConfigLocation.
    /// </summary>
    /// <returns>0 if Config saved successfully, 1 if an error occurs.</returns>
    public int SaveConfig()
    {
        var options = new JsonSerializerOptions { WriteIndented = true,  };
        string configOutput = JsonSerializer.Serialize(this, options);
        try
        {
            File.WriteAllText(ConfigLocation, configOutput);
        }
        catch (Exception e)
        {
            // TODO: Pass to UI error handler  or return error for error handling.
            Console.WriteLine(e);
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// Loads a config from a .json file.
    /// </summary>
    /// <param name="configLocation">Location that configuration is stored.</param>
    /// <returns>Loaded Config.</returns>
    /// <exception cref="JsonException">Thrown if Json is invalid.</exception>
    public static Config LoadConfig(string configLocation)
    {
        // Create default config if none exists.
        if (!File.Exists(configLocation))
        {
            Config newConfig = new Config(new List<Hotspot>());
            newConfig.SaveConfig();
            return newConfig;
        }

        try
        {
            string configJson = File.ReadAllText(configLocation);
            Config? config = JsonSerializer.Deserialize<Config>(configJson);
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
