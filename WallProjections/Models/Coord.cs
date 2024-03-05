namespace WallProjections.Models;

public record Coord(double X, double Y, double R)
{
    /// <summary>
    /// Converts the coordinates to use diameter instead of radius
    /// </summary>
    public ViewCoord ToViewCoord() => new ViewCoord(X, Y, 2 * R);
}

public record ViewCoord(double X, double Y, double D);
