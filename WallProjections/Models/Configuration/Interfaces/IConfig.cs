using System.IO;

namespace WallProjections.Models.Configuration.Interfaces;

public interface IConfig
{
    /// <summary>
    /// Returns hotspot if Id matches a hotspot.
    /// </summary>
    /// <param name="id">Id to match Hotspot</param>
    /// <returns><see cref="Hotspot"/> with matching Id if exists, or null if no such hotspot.</returns>
    public Hotspot? GetHotspot(int id);

    /// <summary>
    /// Number of hotspots stored in <see cref="Config"/>.
    /// </summary>
    /// <returns><see cref="int"/> of count of hotspots in config.</returns>
    public int HotspotCount { get; }
}
