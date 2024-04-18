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
    /// The current state of the hotspot projection
    /// </summary>
    public HotspotState State { get; set; }
}

/// <summary>
/// An enum to hold the current state of the hotspot, to be used to trigger the animations
/// </summary>
public enum HotspotState
{
    None,
    Activating,
    Active,
    Deactivating
}
