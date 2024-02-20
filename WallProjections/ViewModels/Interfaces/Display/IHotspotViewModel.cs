using System.Collections.Immutable;
using WallProjections.Models;

namespace WallProjections.ViewModels.Interfaces.Display;

/// <summary>
/// A viewmodel for projecting hotspots onto the artifact
/// </summary>
public interface IHotspotViewModel
{
    /// <summary>
    /// A list of <see cref="HotspotProjection">projections</see> of hotspots to be displayed
    /// </summary>
    public ImmutableList<HotspotProjection> Projections { get; }

    /// <summary>
    /// Decides whether or not to display the hotspots
    /// </summary>
    public bool IsVisible { get; }

    /// <summary>
    /// Changes the <see cref="HotspotProjection.IsActive"/> parameter for all
    /// <see cref="HotspotProjection"/> to false in <see cref="Projections"/>
    /// and sets the <see cref="HotspotProjection.IsActive"/> parameter for <see cref="HotspotProjection"/>
    /// to true in <see cref="Projections"/> where <see cref="HotspotProjection.Id"/> matches
    /// the id passed into the function
    /// </summary>
    /// <param name="id">The id of the hotspot to be activated</param>
    public void ActivateHotspot(int id);

    /// <summary>
    /// Changes the <see cref="HotspotProjection.IsActive"/> parameter for the
    /// <see cref="HotspotProjection"/> to false for all <see cref="Projections"/>
    /// (to be used when user doesn't hover over hotspot for long enough)
    /// </summary>
    public void DeactivateHotspots();

    /// <summary>
    /// Changes <see cref="IsVisible" /> to true
    /// </summary>
    public void DisplayHotspots();
}
