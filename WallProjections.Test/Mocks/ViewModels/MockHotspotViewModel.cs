using System.Collections.Immutable;
using WallProjections.Models;
using WallProjections.ViewModels;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.Test.Mocks.ViewModels;

/// <summary>
/// A mock of <see cref="HotspotViewModel" />
/// </summary>
public class MockHotspotViewModel : ViewModelBase, IHotspotViewModel
{
    /// <inheritdoc/>
    public ImmutableList<HotspotProjection> Projections { get; } = ImmutableList.Create(
        new HotspotProjection { Id = 0, X = 10, Y = 10, D = 20, IsActive = false },
        new HotspotProjection { Id = 1, X = 90, Y = 130, D = 60, IsActive = false },
        new HotspotProjection { Id = 2, X = 120, Y = 50, D = 40, IsActive = false }
    );

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
        Projections.First().IsActive = true;
    }

    /// <summary>
    /// Mock version of the DeactivateHotspot function in <see cref="HotspotViewModel"/> which just
    /// sets the first hotspot in the list to false
    /// </summary>
    public void DeactivateHotspots()
    {
        Projections.First().IsActive = false;
    }

    /// <inheritdoc/>
    public void DisplayHotspots()
    {
        IsVisible = true;
    }
}
