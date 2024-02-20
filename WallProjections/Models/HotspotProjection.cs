namespace WallProjections.Models;

/// <summary>
/// A record of all the parameters required to display the hotspots
/// </summary>
/// <param name="Id">The id of the hotspot to be activated</param>
/// <param name="X">The number of pixels from the leftmost side</param>
/// <param name="Y">The number of pixels from the top</param>
/// <param name="R">The radius of the hotspot</param>
/// <param name="D">The diameter of the hotspot</param>
/// <param name="IsActive">Shows whether the hotspot is activated or not</param>
public record HotspotProjection(int Id, double X, double Y, double R, double D, bool IsActive);
