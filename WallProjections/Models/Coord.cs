using WallProjections.Models.Interfaces;

namespace WallProjections.Models;

/// <summary>
/// Coordinates and radius of a hotspot in the projector space
/// </summary>
/// <param name="X">The X coordinate of the center of the hotspot in pixels</param>
/// <param name="Y">The Y coordinate of the center of the hotspot in pixels</param>
/// <param name="R">The radius of the hotspot in pixels</param>
public record Coord(double X, double Y, double R) : IPosition
{
    //Default Constructor
    public Coord() : this(0, 0, 30)
    {
    }
}
