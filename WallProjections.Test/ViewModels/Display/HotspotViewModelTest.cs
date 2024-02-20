using System.Collections.Immutable;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Display;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.Test.ViewModels.Display;

[TestFixture]
public class HotspotViewModelTest
{
    /// <summary>
    /// The tolerance for comparing floating point numbers (i.e. hotspot positions)
    /// </summary>
    private const double FpCompTolerance = 0.001;

    /// <summary>
    /// Creates a new config with 3 hotspots
    /// </summary>
    /// <returns></returns>
    private static IConfig CreateConfig()
    {
        var hotspots = new[]
        {
            new Coord(10, 10, 10),
            new Coord(50, 50, 14),
            new Coord(100, 200, 20)
        };
        return new Config(hotspots.Select((coord, i) => new Hotspot(
            i,
            coord,
            "",
            "",
            ImmutableList<string>.Empty,
            ImmutableList<string>.Empty)
        ));
    }

    [Test]
    public void ConstructorTest()
    {
        var config = CreateConfig();
        var hotspotViewModel = new HotspotViewModel(config);
        Assert.That(
            hotspotViewModel.Projections,
            Is.EquivalentTo(config.Hotspots).Using<HotspotProjection, Hotspot>((actual, expected) =>
            {
                var id = actual.Id == expected.Id;
                var x = Math.Abs(actual.X - expected.Position.X) < FpCompTolerance;
                var y = Math.Abs(actual.Y - expected.Position.Y) < FpCompTolerance;
                var d = Math.Abs(actual.D - 2 * expected.Position.R) < FpCompTolerance;
                return id && x && y && d;
            })
        );
    }

    [Test]
    public void ActivateDeactivateHotspotsTest()
    {
        var config = CreateConfig();
        var hotspotViewModel = new HotspotViewModel(config);

        hotspotViewModel.ActivateHotspot(0);
        AssertActiveHotspot(hotspotViewModel, 0);

        hotspotViewModel.ActivateHotspot(1);
        AssertActiveHotspot(hotspotViewModel, 1);

        hotspotViewModel.DeactivateHotspots();
        AssertActiveHotspot(hotspotViewModel, null);
    }

    [Test]
    public void DisplayHotspotsTest()
    {
        var hotspotViewModel = new HotspotViewModel(CreateConfig());
        Assert.That(hotspotViewModel.IsVisible, Is.False);
        hotspotViewModel.DisplayHotspots();
        Assert.That(hotspotViewModel.IsVisible, Is.True);
    }

    /// <summary>
    /// Asserts that the active hotspot is the one with the given id, and that all other hotspots are inactive.
    /// </summary>
    /// <param name="vm">The source of the hotspots.</param>
    /// <param name="activeId">
    /// The id of the hotspot that should be active. If <i>null</i>, no hotspots should be active.
    /// </param>
    private static void AssertActiveHotspot(IHotspotViewModel vm, int? activeId)
    {
        var projections = vm.Projections.GroupBy(h => h.IsActive).ToImmutableList();
        var active = projections.Where(g => g.Key).SelectMany(g => g).ToImmutableList();
        var inactive = projections.Where(g => !g.Key).SelectMany(g => g).ToImmutableList();
        var expectedActiveCount = activeId is null ? 0 : 1;
        Assert.Multiple(() =>
        {
            Assert.That(active, Has.Count.EqualTo(expectedActiveCount));
            Assert.That(inactive, Has.Count.EqualTo(vm.Projections.Count - expectedActiveCount));
        });
        if (activeId is not null)
            Assert.That(active[0].Id, Is.EqualTo(activeId));
    }
}
