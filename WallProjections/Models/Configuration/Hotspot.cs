using System;
using System.Text.Json.Serialization;

namespace WallProjections.Models.Configuration;

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
    /// Record of coordinates for hotspot position (X, Y, Radius)
    /// </summary>
    [JsonInclude]
    public Coord Position { get; }



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
        double x = default,
        double y = default,
        double r = default
        )
    {
        Id = id;
        Position = new Coord(x, y, r);
    }

    [JsonConstructor]
    public Hotspot(int id, Coord position): this(id, position.X, position.Y, position.R) {}
}
