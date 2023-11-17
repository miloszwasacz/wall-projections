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
    /// List of all hotspots (their locations and content).
    /// </summary>
    [JsonInclude]
    public ImmutableList<Hotspot> Hotspots { get; }

    public int HotspotCount => Hotspots.Count;

    /// <summary>
    /// Constructs a new Config object using list of hotspots and a custom location.
    /// </summary>
    /// <param name="hotspots">Collection of hotspots to create config with.</param>
    public Config(IEnumerable<Hotspot> hotspots)
    {
        Hotspots = hotspots.ToImmutableList();
    }

    /// <summary>
    /// Specific constructor so deserializer matches parameters correctly.
    /// </summary>
    /// <param name="hotspots">List of hotspots.</param>
    [JsonConstructor]
    public Config(ImmutableList<Hotspot> hotspots)
    {
        Hotspots = hotspots;
    }
    
    public Hotspot? GetHotspot(int id)
    {
        return Hotspots.Find(x => x.Id == id);
    }
}
