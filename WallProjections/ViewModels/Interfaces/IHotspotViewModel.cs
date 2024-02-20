using System.Collections.Generic;
using WallProjections.Models;

namespace WallProjections.ViewModels.Interfaces;

public interface IHotspotViewModel
{
    /// <summary>
    /// A list of coordinates and diameters of hotspots given through the config
    /// </summary>
    public List<HotspotProjection> Coordinates { get; }

    /// <summary>
    /// Decides whether or not to display the hotspots
    /// </summary>
    public bool ShowHotspots { get; }
    
    /// <summary>
    /// Changes the <see cref="HotspotProjection.IsActive"/> parameter for all
    /// <see cref="HotspotProjection"/> to false in <see cref="Coordinates"/>
    /// and sets the <see cref="HotspotProjection.IsActive"/> parameter for <see cref="HotspotProjection"/>
    /// to true in <see cref="Coordinates"/> where <see cref="HotspotProjection.Id"/> matches
    /// the id passed into the function
    /// </summary>
    /// <param name="id">The id of the hotspot to be activated</param>
    public void ActivateHotspot(int id);

    /// <summary>
    /// Changes the <see cref="HotspotProjection.IsActive"/> parameter for the
    /// <see cref="HotspotProjection"/> to false for all <see cref="Coordinates"/>
    /// (to be used when user doesn't hover over hotspot for long enough)
    /// </summary>
    public void DeactivateHotspots();
    
    /// <summary>
    /// Changes <see cref="ShowHotspots" /> to true
    /// </summary>
    public void DisplayHotspots();
}
