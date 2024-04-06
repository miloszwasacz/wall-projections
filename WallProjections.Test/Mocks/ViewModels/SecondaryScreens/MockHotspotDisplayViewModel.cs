using System.Collections.Immutable;
using WallProjections.Models;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.Test.Mocks.ViewModels.SecondaryScreens;

/// <summary>
/// A mock of <see cref="HotspotDisplayViewModel" />
/// </summary>
public class MockHotspotDisplayViewModel : AbsHotspotDisplayViewModel, IDisposable
{
    /// <summary>
    /// Whether <see cref="Dispose" /> has been called
    /// </summary>
    public bool IsDisposed { get; private set; }

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
    /// Mock version of the DeactivateHotspot function in <see cref="HotspotDisplayViewModel"/> which just
    /// sets <see cref="IHotspotProjectionViewModel.IsActive" /> of the first hotspot in the list to false
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

    public void Dispose()
    {
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
