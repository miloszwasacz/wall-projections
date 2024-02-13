using System.Collections.Generic;
using WallProjections.Models;

namespace WallProjections.ViewModels.Interfaces;

public interface IHotspotViewModel
{
    /// <summary>
    /// A list of coordinates and diameters of hotspots given through the config
    /// </summary>
    public List<HotCoord> Coordinates { get; }

    /// <summary>
    /// Decides whether or not to display the hotspots
    /// </summary>
    public bool ShowHotspots { get; }
    
    /// <summary>
    /// Changes the Vis parameter for all
    /// <see cref="HotCoord"/> to false in <see cref="Coordinates"/>
    /// and sets the Vis parameter for <see cref="HotCoord"/>
    /// to true in <see cref="Coordinates"/> where Id matches
    /// the id passed into the function
    /// </summary>
    public void ActivateHotspot(int id);

    /// <summary>
    /// Changes <see cref="ShowHotspots" /> to true
    /// </summary>
    public void DisplayHotspots();
}
