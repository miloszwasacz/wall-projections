using WallProjections.Models;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Interfaces;

namespace WallProjections.Test.Mocks.ViewModels;

/// <summary>
/// A mock of <see cref="HotspotViewModel" />
/// </summary>

public class MockHotspotViewModel: ViewModelBase, IHotspotViewModel
{
    /// <inheritdoc/>
    public List<HotCoord> Coordinates { get; private set; } = new();

    /// <inheritdoc/>
    public bool ShowHotspots { get; private set; }

    /// <summary>
    /// mock version of the ActivateHotspot function in <see cref="HotspotViewModel"/> which just
    /// sets the first hotspot in the list to true
    /// </summary>
    public void ActivateHotspot(int id)
    {
        var toChange = Coordinates.First();
        var active = new HotCoord(toChange.Id, toChange.X, toChange.Y, toChange.R, toChange.D, true);
        var newCoords = new List<HotCoord> { active };
        newCoords.AddRange(Coordinates.Where(coord => coord.Id != toChange.Id));
        Coordinates = newCoords;
    }

    /// <inheritdoc/>
    public void DisplayHotspots()
    {
        ShowHotspots = true;
    }
}
