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
    public List<Hotspot> Hotspots { get; }

    /// <summary>
    /// Location where to store configuration.
    /// </summary>
    public string ConfigLocation { get; }

    /// <summary>
    /// Saves the current state of Config to the file path from ConfigLocation.
    /// Throws exception if error occurs on save.
    /// </summary>
    /// <exception cref="PathTooLongException">Config path too long for system.</exception>
    /// <exception cref="DirectoryNotFoundException">Directory to save config is invalid.</exception>
    /// <exception cref="IOException">Error occurs when trying to access file.</exception>
    /// <exception cref="UnauthorizedAccessException">
    ///     Program does not have sufficient permissions to write config.
    /// </exception>
    /// <exception cref="NotSupportedException">Config path is invalid.</exception>
    public void SaveConfig()
    {
        var options = new JsonSerializerOptions { WriteIndented = true,  };
        var configOutput = JsonSerializer.Serialize(this, options);

        File.WriteAllText(ConfigLocation, configOutput);
    }

    /// <summary>
    /// Loads a config from a .json file.
    /// </summary>
    /// <param name="configLocation">Location that configuration is stored.</param>
    /// <returns>Loaded Config.</returns>
    /// <exception cref="JsonException">Format of config file is invalid.</exception>
    public static Config LoadConfig(string configLocation)
    {
        // Create default config if none exists.
        if (!File.Exists(configLocation))
        {
            var newConfig = new Config(new List<Hotspot>());
            newConfig.SaveConfig();
            return newConfig;
        }

        try
        {
            var configJson = File.ReadAllText(configLocation);
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

    /// <summary>
    /// Returns hotspot if Id matches a hotspot.
    /// </summary>
    /// <param name="id">Id to match Hotspot</param>
    /// <returns>Hotspot with matching Id if exists, or null if no such Hotspot.</returns>
    public Hotspot? GetHotspot(int id)
    {
        return Hotspots.Find(x => x.Id == id);
    }

    /// <summary>
    /// Default constructor for Config for if no hotspot.
    /// </summary>
    /// <param name="configLocation">Path to save config file.</param>
    public Config(string configLocation = "config.json")
    {
        Hotspots = new List<Hotspot>();
        ConfigLocation = configLocation;
    }

    /// <summary>
    /// Constructs a new Config object using list of hotspots and a custom location.
    /// </summary>
    /// <param name="hotspots">Collection of hotspots to create config with.</param>
    /// <param name="configLocation">Path to save config location.</param>
    public Config(IEnumerable<Hotspot>? hotspots, string configLocation = "config.json")
    {
        ConfigLocation = configLocation;
        hotspots ??= new List<Hotspot>();
        Hotspots = new List<Hotspot>(hotspots);
    }

    /// <summary>
    /// Specific constructor so deserializer matches parameters correctly.
    /// </summary>
    /// <param name="hotspots">List of hotspots.</param>
    /// <param name="configLocation">Location to store config file.</param>
    [JsonConstructor]
    public Config(List<Hotspot> hotspots, string configLocation)
        : this(hotspots.AsReadOnly(), configLocation) {}
}
