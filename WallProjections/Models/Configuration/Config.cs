using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using WallProjections.Models.Configuration.Interfaces;

namespace WallProjections.Models.Configuration;

/// <summary>
/// Stores all user customisable configuration for the program.
/// </summary>
[Serializable]
public class Config : IConfig
{
    /// <summary>
    /// Backing field for <see cref="Hotspots"/>
    /// </summary>
    [JsonIgnore]
    private List<Hotspot> _hotspots;

    /// <summary>
    /// List of all hotspots (their locations and content).
    /// </summary>
    [JsonInclude]
    public ImmutableList<Hotspot> Hotspots => _hotspots.ToImmutableList();

    public string ConfigLocation { get; }

    [JsonIgnore]
    public static string TempPath => IConfig.TempPath;

    /// <summary>
    /// Default constructor for Config for if no hotspot.
    /// </summary>
    /// <param name="configLocation">Path to save config file.</param>
    public Config(string configLocation = "config.json")
    {
        _hotspots = new List<Hotspot>();
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
        _hotspots = new List<Hotspot>(hotspots);
    }

    /// <summary>
    /// Specific constructor so deserializer matches parameters correctly.
    /// </summary>
    /// <param name="hotspots">List of hotspots.</param>
    /// <param name="configLocation">Location to store config file.</param>
    [JsonConstructor]
    public Config(ImmutableList<Hotspot> hotspots, string configLocation)
    {
        ConfigLocation = configLocation;
        _hotspots = new List<Hotspot>(hotspots);
    }
    
    public Hotspot? GetHotspot(int id)
    {
        return _hotspots.Find(x => x.Id == id);
    }

    public int HotspotCount()
    {
        return _hotspots.Count;
    }

    /// <summary>
    /// Saves the current state of Config to the file name stored in <see cref="ConfigLocation"/>.
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
    /// Saves the current state of Config to the file name stored in <see cref="ConfigLocation"/> using path.
    /// Throws exception if error occurs on save.
    /// </summary>
    /// <param name="path">Path to store config file in.</param>
    public void SaveConfig(string path)
    {
        var options = new JsonSerializerOptions { WriteIndented = true,  };
        var configOutput = JsonSerializer.Serialize(this, options);

        File.WriteAllText(Path.Combine(path, ConfigLocation), configOutput);
    }
}
