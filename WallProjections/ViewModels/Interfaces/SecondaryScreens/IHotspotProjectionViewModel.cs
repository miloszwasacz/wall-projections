namespace WallProjections.ViewModels.Interfaces.SecondaryScreens;

/// <summary>
/// A viewmodel holding all info required to project a hotspot onto the artifact
/// </summary>
public interface IHotspotProjectionViewModel
{
    /// <summary>
    /// The id of the hotspot to be activated
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The number of pixels from the leftmost side
    /// </summary>
    public double X { get; }

    /// <summary>
    /// The number of pixels from the top
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// The diameter of the hotspot
    /// </summary>
    public double D { get; }

    /// <summary>
    /// Shows whether the hotspot is activated or not
    /// </summary>
    public bool IsActive { get; set; }
}
