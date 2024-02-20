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
    public List<HotspotProjection> Coordinates { get; private set; } = new();

    /// <inheritdoc/>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// Mock version of the ActivateHotspot function in <see cref="HotspotViewModel"/> which just
    /// sets the first hotspot in the list to true, takes in the param <paramref name="id"></paramref>
    /// to uphold the interface but does not use this parameter 
    /// </summary>
    /// <param name="id">The id of the hotspot to be activated</param>
    public void ActivateHotspot(int id)
    {
        Coordinates.First().IsActive = true;
    }

    /// <summary>
    /// Mock version of the DeactivateHotspot function in <see cref="HotspotViewModel"/> which just
    /// sets the first hotspot in the list to false
    /// </summary>
    public void DeactivateHotspots()
    {
        Coordinates.First().IsActive = false;
    }

    /// <inheritdoc/>
    public void DisplayHotspots()
    {
        IsVisible = true;
    }
}
