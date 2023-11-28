using System;

namespace WallProjections.Models.Interfaces;

public interface IConfig
{
    /// <summary>
    /// Returns hotspot if Id matches a hotspot.
    /// </summary>
    /// <param name="id">Id to match Hotspot</param>
    /// <returns><see cref="Hotspot"/> with matching Id if exists, or null if no such hotspot.</returns>
    public Hotspot? GetHotspot(int id);

    /// <summary>
    /// Number of hotspots stored in <see cref="IConfig">Config</see>.
    /// </summary>
    public int HotspotCount { get; }

    public class HotspotNotFoundException : Exception
    {
        public HotspotNotFoundException(int id) : base($"Hotspot with ID {id} not found.")
        {
        }
    }
}
