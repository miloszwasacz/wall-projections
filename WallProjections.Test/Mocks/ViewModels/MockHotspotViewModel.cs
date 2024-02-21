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
    public ImmutableList<HotspotProjectionViewModel> Projections { get; } = new[]
    {
        (0, 10.0, 10.0, 10.0),
        (1, 90.0, 130.0, 30.0),
        (2, 120.0, 50.0, 20.0)
    }.Select(data =>
    {
        var (id, x, y, r) = data;
        var hotspot = new Hotspot(
            id,
            new Coord(x, y, r),
            "",
            "",
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty
        );
        return new HotspotProjectionViewModel(hotspot);
    }).ToImmutableList();

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
