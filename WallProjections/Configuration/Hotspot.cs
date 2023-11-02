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
    /// X value of the hotspot.
    /// </summary>
    [JsonInclude]
    public double X { get; set; }

    /// <summary>
    /// Y value of the hotspot.
    /// </summary>
    [JsonInclude]
    public double Y { get; set; }

    /// <summary>
    /// Radius of the hotspot.
    /// </summary>
    [JsonInclude]
    public double R { get; set; }

    public Hotspot(double x, double y, double r)
    {
        X = x;
        Y = y;
        R = r;
    }
}
