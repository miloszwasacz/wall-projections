using System;
using System.Text.Json.Serialization;

namespace WallProjections.Configuration;

/// <summary>
/// Stores the location of a hotspot, its size, and the content to be displayed.
/// </summary>
[Serializable]
public class Hotspot
{
    /// <summary>
    /// ID of the hotspot. Used by input to tell UI which hotspot info to show.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// X value of the hotspot.
    /// </summary>
    [JsonInclude]
    public double? X { get; }

    /// <summary>
    /// Y value of the hotspot.
    /// </summary>
    [JsonInclude]
    public double? Y { get; }

    /// <summary>
    /// Radius of the hotspot.
    /// </summary>
    [JsonInclude]
    public double? R { get; }

    /// <summary>
    /// Constructor for Hotspot
    /// </summary>
    /// <param name="id">ID used by input detection to show info.</param>
    /// <param name="x">X value for hotspot in camera. If not default, then </param>
    /// <param name="y">Y value for hotspot in camera.</param>
    /// <param name="r">Radius for hotspot in camera.</param>
    /// <exception cref="ArgumentException">Thrown if both image + video at once, or no content defined.</exception>
    public Hotspot(
        int id,
        double? x = default,
        double? y = default,
        double? r = default
        )
    {
        Id = id;
        X = x;
        Y = y;
        R = r;
    }
}
