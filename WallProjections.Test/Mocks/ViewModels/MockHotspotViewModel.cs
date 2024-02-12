using ReactiveUI;
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
    /// The backing field for <see cref="ShowHotspots" />
    /// </summary>
    private bool _displayHotspots = false;

    /// <summary>
    /// A list of coordinates and diameters of hotspots given through the config
    /// </summary>
    public List<HotCoord> Coordinates { get; private set; } = new();

    /// <summary>
    /// Decides whether or not to display the hotspots
    /// </summary>
    public bool ShowHotspots
    {
        get => _displayHotspots;
        private set => this.RaiseAndSetIfChanged(ref _displayHotspots, value);
    } 
    
    /// <summary>
    /// Changes the <see paramcref="Vis" /> parameter for all
    /// <see cref="HotCoord"/> to false in <see cref="Coordinates"/>
    /// and sets <see paramcref="Vis" /> parameter for <see cref="HotCoord"/>
    /// to true in <see cref="Coordinates"/> where <see paramcref="Id" /> matches
    /// the id passed into the function
    /// </summary>
    public void ActivateHotspot(int id)
    {
        var updatedCoords = new List<HotCoord>();
        foreach (var coord in Coordinates)
        {
            if (coord.Vis) {
                var newCoord = new HotCoord(coord.Id, coord.X, coord.Y, coord.R, coord.D, false);
                updatedCoords.Add(newCoord);
            } 
            else if (coord.Id == id) 
            {
                var newCoord = new HotCoord(coord.Id, coord.X, coord.Y, coord.R, coord.D, true);
                updatedCoords.Add(newCoord);
            } 
            else {
                updatedCoords.Add(coord);
            }
        }
        Coordinates = updatedCoords;
    }

    /// <summary>
    /// Changes <see cref="ShowHotspots" /> to true
    /// </summary>
    public void DisplayHotspots()
    {
        _displayHotspots = true;
    }
}
