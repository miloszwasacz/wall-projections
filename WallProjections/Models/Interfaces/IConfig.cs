using System;
using System.Collections.Immutable;

namespace WallProjections.Models.Interfaces;

public interface IConfig
{
    /// <summary>
    /// A 3x3 matrix used for camera calibration.
    /// </summary>
    public float[,] HomographyMatrix { get; }

    /// <summary>
    /// List of all hotspots (their locations and content).
    /// </summary>
    public ImmutableList<Hotspot> Hotspots { get; }

    /// <summary>
    /// Returns hotspot if Id matches a hotspot.
    /// </summary>
    /// <param name="id">Id to match Hotspot</param>
    /// <returns><see cref="Hotspot"/> with matching Id if exists, or null if no such hotspot.</returns>
    public Hotspot? GetHotspot(int id);

    public class HotspotNotFoundException : Exception
    {
        public HotspotNotFoundException(int id) : base($"Hotspot with ID {id} not found.")
        {
        }
    }
}
