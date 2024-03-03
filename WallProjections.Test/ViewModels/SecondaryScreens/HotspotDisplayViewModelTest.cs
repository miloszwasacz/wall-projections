using System.Collections.Immutable;
using WallProjections.Models;
using WallProjections.Models.Interfaces;
using WallProjections.Test.Mocks.Helper;
using WallProjections.Test.Mocks.ViewModels;
using WallProjections.Test.Mocks.ViewModels.SecondaryScreens;
using WallProjections.ViewModels.Interfaces.SecondaryScreens;
using WallProjections.ViewModels.SecondaryScreens;

namespace WallProjections.Test.ViewModels.SecondaryScreens;

[TestFixture]
public class HotspotDisplayViewModelTest
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// A mock viewmodel provider
    /// </summary>
    private static readonly MockViewModelProvider VMProvider = new();

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
        return new Config(new float[3, 3], hotspots.Select((coord, i) => new Hotspot(
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
        var pythonHandler = new MockPythonHandler();
        var hotspotViewModel = new HotspotDisplayViewModel(config, pythonHandler, VMProvider);
        Assert.Multiple(() =>
        {
            Assert.That(
                hotspotViewModel.Projections,
                Is.EquivalentTo(config.Hotspots).Using<IHotspotProjectionViewModel, Hotspot>(
                    (actual, expected) => actual.IsSameAsHotspot(expected)
                )
            );
            //TODO Add this assertion when the hiding has been properly implemented
            // Assert.That(hotspotViewModel.IsVisible, Is.False);
        });
    }

    [Test]
    public async Task ActivateDeactivateHotspotsTest()
    {
        var config = CreateConfig();
        var pythonHandler = new MockPythonHandler();
        var hotspotViewModel = new HotspotDisplayViewModel(config, pythonHandler, VMProvider);

        // Activate directly
        hotspotViewModel.ActivateHotspot(0);
        AssertActiveHotspot(hotspotViewModel, 0);

        // Activate through the PythonHandler
        pythonHandler.OnHotspotPressed(1);
        await Task.Delay(50);
        AssertActiveHotspot(hotspotViewModel, 1);

        // Deactivate all hotspots
        hotspotViewModel.DeactivateHotspots();
        AssertActiveHotspot(hotspotViewModel, null);
    }

    //TODO Enable this test when the hiding has been properly implemented
    [Test]
    [Ignore("Hiding has not yet been properly implemented")]
    public void DisplayHotspotsTest()
    {
        var pythonHandler = new MockPythonHandler();
        var hotspotViewModel = new HotspotDisplayViewModel(CreateConfig(), pythonHandler, VMProvider);
        Assert.That(hotspotViewModel.IsVisible, Is.False);
        hotspotViewModel.DisplayHotspots();
        Assert.That(hotspotViewModel.IsVisible, Is.True);
    }

    [Test]
    public void DisposeTest()
    {
        var pythonHandler = new MockPythonHandler();
        var hotspotViewModel = new HotspotDisplayViewModel(CreateConfig(), pythonHandler, VMProvider);
        AssertActiveHotspot(hotspotViewModel, null);

        hotspotViewModel.Dispose();

        // The event is ignored, so it should not have any effect
        pythonHandler.OnHotspotPressed(0);
        AssertActiveHotspot(hotspotViewModel, null);
    }

    /// <summary>
    /// Asserts that the active hotspot is the one with the given id, and that all other hotspots are inactive.
    /// </summary>
    /// <param name="vm">The source of the hotspots.</param>
    /// <param name="activeId">
    /// The id of the hotspot that should be active. If <i>null</i>, no hotspots should be active.
    /// </param>
    private static void AssertActiveHotspot(IHotspotDisplayViewModel vm, int? activeId)
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
