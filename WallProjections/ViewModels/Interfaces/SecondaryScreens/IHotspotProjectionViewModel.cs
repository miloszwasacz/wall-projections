using WallProjections.Models.Interfaces;

namespace WallProjections.ViewModels.Interfaces.SecondaryScreens;

/// <summary>
/// A viewmodel holding all info required to project a hotspot onto the artifact
/// </summary>
/// <remarks>
/// <see cref="IPosition.X" /> and <see cref="IPosition.Y"/> are the X and Y coordinates
/// of the top-left corner of the hotspot's bounding box
/// </remarks>
public interface IHotspotProjectionViewModel : IPosition
{
    /// <summary>
    /// The id of the hotspot to be activated
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The diameter of the hotspot
    /// </summary>
    public double D { get; }

    /// <summary>
    /// Whether the hotspot is being activated
    /// </summary>
    /// <remarks>
    /// Note that mutating <see cref="IsActive" /> or <see cref="IsDeactivating" /> does not change this property
    /// </remarks>
    public bool IsActivating { get; set; }

    /// <summary>
    /// Whether the hotspot is being deactivated
    /// </summary>
    /// <remarks>
    /// Note that mutating <see cref="IsActive" /> or <see cref="IsActivating" /> does not change this property
    /// </remarks>
    public bool IsDeactivating { get; set; }

    /// <summary>
    /// Whether the hotspot is fully activated
    /// </summary>
    /// <remarks>
    /// Note that mutating <see cref="IsActivating" /> or <see cref="IsDeactivating" /> does not change this property
    /// </remarks>
    public bool IsActive { get; set; }
}
