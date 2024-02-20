namespace WallProjections.Models;

/// <summary>
/// A record of all the parameters required to display the hotspots
/// </summary>
public record HotspotProjection
{
    /// <summary>
    /// The id of the hotspot to be activated
    /// </summary>
    public int Id { get; init; }
    
    /// <summary>
    /// The number of pixels from the leftmost side
    /// </summary>
    public double X { get; init; }
    
    /// <summary>
    /// The number of pixels from the top
    /// </summary>
    public double Y { get; init; }
    
    /// <summary>
    /// The diameter of the hotspot
    /// </summary>
    public double D { get; init; }
    
    /// <summary>
    /// Shows whether the hotspot is activated or not
    /// </summary>
    public bool IsActive { get; set; }
}
