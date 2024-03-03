using System.Collections.Immutable;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.Test.Mocks.ViewModels.SecondaryScreens;

/// <summary>
/// A mock of <see cref="HotspotDisplayViewModel" />
/// </summary>
public class MockHotspotDisplayViewModel : IHotspotDisplayViewModel
{
    /// <inheritdoc/>
    public override ImmutableList<IHotspotProjectionViewModel> Projections { get; } = new[]
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
        return new HotspotProjectionViewModel(hotspot) as IHotspotProjectionViewModel;
    }).ToImmutableList();

    /// <inheritdoc/>
    public override bool IsVisible { get; protected set; }

    /// <summary>
    /// Mock version of the ActivateHotspot function in <see cref="HotspotDisplayViewModel"/> which just
    /// sets the first hotspot in the list to true, takes in the param <paramref name="id"></paramref>
    /// to uphold the interface but does not use this parameter 
    /// </summary>
    /// <param name="id">The id of the hotspot to be activated</param>
    public override void ActivateHotspot(int id)
    {
        Projections.First().IsActive = true;
    }

    /// <summary>
    /// Mock version of the DeactivateHotspot function in <see cref="HotspotDisplayViewModel"/> which just
    /// sets the first hotspot in the list to false
    /// </summary>
    public override void DeactivateHotspots()
    {
        Projections.First().IsActive = false;
    }

    /// <inheritdoc/>
    public override void DisplayHotspots()
    {
        IsVisible = true;
    }
}
