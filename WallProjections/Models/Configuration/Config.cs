using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    public int HotspotCount => _hotspots.Count;

    /// <summary>
    /// Default constructor for Config for if no hotspot.
    /// </summary>
    public Config()
    {
        _hotspots = new List<Hotspot>();
    }

    /// <summary>
    /// Constructs a new Config object using list of hotspots and a custom location.
    /// </summary>
    /// <param name="hotspots">Collection of hotspots to create config with.</param>
    public Config(IEnumerable<Hotspot>? hotspots)
    {
        hotspots ??= new List<Hotspot>();
        _hotspots = new List<Hotspot>(hotspots);
    }

    /// <summary>
    /// Specific constructor so deserializer matches parameters correctly.
    /// </summary>
    /// <param name="hotspots">List of hotspots.</param>
    [JsonConstructor]
    public Config(ImmutableList<Hotspot> hotspots)
    {
        _hotspots = new List<Hotspot>(hotspots);
    }
    
    public Hotspot? GetHotspot(int id)
    {
        return _hotspots.Find(x => x.Id == id);
    }
}
