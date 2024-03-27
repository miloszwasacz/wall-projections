namespace WallProjections.Models.Interfaces;

/// <summary>
/// A position in 2D space.
/// </summary>
public interface IPosition
{
    /// <summary>
    /// The X coordinate of the position.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// The Y coordinate of the position.
    /// </summary>
    public double Y { get; }
}
