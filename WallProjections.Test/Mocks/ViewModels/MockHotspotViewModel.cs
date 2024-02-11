using WallProjections.Models;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks.ViewModels;

/// <summary>
/// A mock of <see cref="HotspotViewModel" />
/// </summary>

public class MockHotspotViewModel: ViewModelBase, IHotspotViewModel
{
    /// <summary>
    /// The backing field for <see cref="Coordinates" />
    /// </summary>
    private readonly List<HotCoord> _coordList = new();
    
    /// <summary>
    /// The backing field for <see cref="ShowHotspots" />
    /// </summary>
    private bool _dispHotspots = false;

    /// <summary>
    /// A list of coordinates and diameters of hotspots given through the config
    /// </summary>
    public List<HotCoord> Coordinates => _coordList;

    /// <summary>
    /// Decides whether or not to display the hotspots
    /// </summary>
    public bool ShowHotspots => _dispHotspots;
    
    /// <summary>
    /// Changes the <see paramcref="Vis" /> parameter for all
    /// <see cref="HotCoord"/> to false in <see cref="Coordinates"/>
    /// and sets <see paramcref="Vis" /> parameter for <see cref="HotCoord"/>
    /// to true in <see cref="Coordinates"/> where <see paramcref="Id" /> matches
    /// the id passed into the function
    /// </summary>
    public void ActivateHotspot(int id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Changes <see cref="ShowHotspots" /> to true
    /// </summary>
    public void DisplayHotspots()
    {
        _dispHotspots = true;
    }
}
