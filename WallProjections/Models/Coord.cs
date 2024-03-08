namespace WallProjections.Models;

/// <summary>
/// Coordinates and radius of a hotspot
/// </summary>
/// <param name="X">The number of pixels from the leftmost side</param>
/// <param name="Y">The number of pixels from the top of display</param>
/// <param name="R">The radius of the hotspot</param>
public record Coord(double X, double Y, double R)
{
    //Default Constructor
    public Coord() : this(0, 0, 30) {}

    /// <summary>
    /// Converts the coordinates to use diameter instead of radius
    /// </summary>
    public ViewCoord ToViewCoord() => new ViewCoord(X, Y, 2 * R);
}

/// <summary>
/// Coordinates and diameter of a hotspot in the projector space
/// </summary>
/// <param name="X">The number of pixels from the leftmost side</param>
/// <param name="Y">The number of pixels from the top of display</param>
/// <param name="D">The diameter of the hotspot</param>
public record ViewCoord(double X, double Y, double D);
